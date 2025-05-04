using AutoMapper;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Entities.GoogleFields.Request;
using DesolaDomain.Entities.GoogleFields.Response;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaInfrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.External.Providers.Google;

public class GoogleFlightProvider : BaseFlightProvider
{
    private readonly RapidApi _googleConfig;
    public override string ProviderName => _googleConfig.GoogleProviderName;
    public GoogleFlightProvider(ICacheService memoryCache, IMapper mapper, ILogger<GoogleFlightProvider> logger, IApiService apiService, IBlobStorageRepository blobStorageRepository, IOptions<AppSettings> settingsOptions)
        : base(memoryCache, mapper, logger, apiService, blobStorageRepository)
    {
        _googleConfig = settingsOptions.Value.ExternalApi.RapidApi;
    }

    protected override async Task<UnifiedFlightSearchResponse> GetFlightOffersAsync(FlightSearchParameters parameters, CancellationToken cancellationToken)
    {
        try
        {
            var googleFlightRequest = Mapper.Map<GoogleFlightRequest>(parameters);
            var endpoint = googleFlightRequest.IsOneWay ? "search-one-way" : "search-roundtrip";

            var headers = new Dictionary<string, string>
            {
                { "x-rapidapi-key", _googleConfig.SkyScannerKey },
                { "x-rapidapi-host", _googleConfig.SkyScannerHost}
            };

            var googleResponse = await ApiService.CallProviderApiAsync<GoogleFlightRequest, GoogleFlightResponse>(
                _googleConfig.GoogleFlightUri,
                endpoint,
                HttpMethod.Get,
                googleFlightRequest, 
                headers, 
                _googleMapper,
                cancellationToken);

            var result = await FlightSearchResult<GoogleFlightResponse>.GetFromMappedApiAsync(googleResponse, Mapper, new { }, cancellationToken);

            return result.UnifiedResponse;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching for flights with Google provider");
            throw;
        }
    }

    private readonly Func<GoogleFlightRequest, IDictionary<string, string>> _googleMapper = criteria =>
    {
        var parameters = new Dictionary<string, string>
        {
            ["departureId"] = criteria.DepartureId,
            ["arrivalId"] = criteria.ArrivalId,
            ["departureDate"] = criteria.DepartureDate,
            ["arrivalDate"] = criteria.ArrivalDate,
            ["cabinClass"] = criteria.CabinClass.ToString(),
            ["stops"] = criteria.StopOver.ToString(),
            ["currency"] = "USD",
            ["sort"] = criteria.FlightSortOption.ToString(),
            ["adults"] = criteria.Adults.ToString(),
        };

        return parameters;
    };

    protected override string GenerateCacheKey(FlightSearchParameters parameters) => $"{ProviderName}_{parameters.Origin}_{parameters.Destination}";

}