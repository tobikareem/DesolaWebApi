using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Interfaces;
using AutoMapper;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Settings;
using Microsoft.Extensions.Options;
using DesolaDomain.Entities.AmadeusFields.Basic;
using Microsoft.Extensions.Logging;
using Desola.Common.Exceptions;
using DesolaDomain.Entities.AmadeusFields.Response;

namespace DesolaInfrastructure.External.Providers.Amadeus;

public class AmadeusFlightProvider : BaseFlightProvider
{
    private readonly AmadeusApi _amadeusConfig;
    private readonly IAirlineRepository _airlineRepository;

    public override string ProviderName => _amadeusConfig.ProviderName;

    public AmadeusFlightProvider(IApiService apiService, IOptions<AppSettings> settingsOptions, ILogger<AmadeusFlightProvider> logger, IMapper mapper, IAirlineRepository airlineRepository, ICacheService memoryCache, IBlobStorageRepository blobStorageRepository)
        : base(memoryCache, mapper, logger, apiService, blobStorageRepository)
    {
        _amadeusConfig = settingsOptions.Value.ExternalApi.Amadeus;
        _airlineRepository = airlineRepository;
    }
    protected override async Task<UnifiedFlightSearchResponse> GetFlightOffersAsync(FlightSearchParameters parameters, CancellationToken cancellationToken)
    {
        try
        {
            var accessToken = await ApiService.FetchAccessTokenAsync(
                _amadeusConfig.TokenEndpointUrl,
                _amadeusConfig.ClientId,
                _amadeusConfig.ClientSecret,
                ProviderName);


            var airlines = await _airlineRepository.GetAllAsync();

            var headers = new Dictionary<string, string>
            {
                { "Authorization", accessToken }
            };

            var amadeusResponse = await ApiService.CallProviderApiAsync<FlightSearchParameters, AmadeusFlightOffersResponse>(
                _amadeusConfig.BaseUrl,
                _amadeusConfig.FlightSearchUrl,
                HttpMethod.Get,
                parameters,
                headers,
                _amadeusMapper,
                cancellationToken);

            var result = await FlightSearchResult<AmadeusFlightOffersResponse>.GetFromMappedApiAsync(amadeusResponse, Mapper, new { Airlines = airlines }, cancellationToken);


            var cacheKey = GenerateCacheKey(parameters);
            SaveToBlobCacheAsync(cacheKey, result.RawResponse, result.UnifiedResponse);

            return result.UnifiedResponse;
        }
        catch (AmadeusApiException ex)
        {
            Logger.LogError("Amadeus API error: {StatusCode} - {ErrorTitle}: {ErrorDetail}",
                ex.StatusCode, ex.ErrorTitle, ex.ErrorDetail);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching for flights with Amadeus provider");
            throw;
        }
    }

    protected override string GenerateCacheKey(FlightSearchParameters parameters) => $"{ProviderName}_{parameters.Origin}_{parameters.Destination}";

    readonly Func<FlightSearchParameters, IDictionary<string, string>> _amadeusMapper = criteria =>
    {
        var parameters = new Dictionary<string, string>
        {
            ["originLocationCode"] = criteria.Origin,
            ["destinationLocationCode"] = criteria.Destination,
            ["departureDate"] = criteria.DepartureDate.ToString("yyyy-MM-dd"),
            ["currencyCode"] = criteria.CurrencyCode
        };

        if (criteria.ReturnDate.HasValue)
        {
            parameters["returnDate"] = criteria.ReturnDate.Value.ToString("yyyy-MM-dd");
        }

        parameters["adults"] = criteria.Adults.ToString();

        if (criteria.Children > 0)
        {
            parameters["children"] = criteria.Children.ToString();
        }

        if (criteria.Infants > 0)
        {
            parameters["infants"] = criteria.Infants.ToString();
        }

        if (!string.IsNullOrEmpty(criteria.CabinClass))
        {
            parameters["travelClass"] = criteria.CabinClass;
        }

        if (criteria.IncludedAirlineCodes.Any())
        {
            parameters["includedAirlineCodes"] = string.Join(",", criteria.IncludedAirlineCodes);
        }

        if (criteria.ExcludedAirlineCodes.Any())
        {
            parameters["excludedAirlineCodes"] = string.Join(",", criteria.ExcludedAirlineCodes);
        }

        if (criteria.NonStop)
        {
            parameters["nonStop"] = "true";
        }


        if (criteria.MaxPrice > 0)
        {
            parameters["maxPrice"] = criteria.MaxPrice.ToString();
        }

        return parameters;
    };
}