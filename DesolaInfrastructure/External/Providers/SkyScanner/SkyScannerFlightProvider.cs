using AutoMapper;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DesolaDomain.Entities.SkyScannerFields;
using DesolaDomain.Aggregates;
using Desola.Common.Exceptions;
using DesolaDomain.Enums;
using DesolaInfrastructure.Services.Base;

namespace DesolaInfrastructure.External.Providers.SkyScanner;

public class SkyScannerFlightProvider : BaseFlightProvider
{
    private readonly RapidApi _skyScannerConfig;
    private readonly Dictionary<string, string> _airlineLogo;
    public override string ProviderName => _skyScannerConfig.SkyScannerProviderName;
    

    public SkyScannerFlightProvider(ILogger<SkyScannerFlightProvider> logger, IOptions<AppSettings> settingsOptions, IMapper mapper, IApiService apiService, ICacheService memoryCache, IBlobStorageRepository blobStorageRepository)
    : base(memoryCache, mapper, logger, apiService, blobStorageRepository)
    {
        _skyScannerConfig = settingsOptions.Value.ExternalApi.RapidApi;
        _airlineLogo = new Dictionary<string, string>();
    }

    protected override async Task<UnifiedFlightSearchResponse> GetFlightOffersAsync(FlightSearchParameters parameters, CancellationToken cancellationToken)
    {
        try
        {

            var skyScannerRequest = Mapper.Map<SkyScannerFlightRequest>(parameters);
            var endpoint = skyScannerRequest.IsOneWay ? "search-one-way" : "search-roundtrip";

            var headers = new Dictionary<string, string>
            {
                { "x-rapidapi-key", _skyScannerConfig.SkyScannerKey },
                { "x-rapidapi-host", _skyScannerConfig.SkyScannerHost}
            };

            var skyScannerResponse = await ApiService.CallProviderApiAsync<SkyScannerFlightRequest, SkyScannerFlightOffer>(
                _skyScannerConfig.SkyScannerUri,
                endpoint,
                HttpMethod.Get,
                skyScannerRequest,
                headers,
                _skyScannerMapper,
                cancellationToken);

            _airlineLogo.Clear();

            // Extract any relevant airline logos from the response
            ExtractAirlineLogos(skyScannerResponse);

            // Update airline logo cache if new logos were found
            UpdateAirlineLogoCache();

            // Use FlightSearchResult to store the raw response and transform to unified format
            var result = await FlightSearchResult<SkyScannerFlightOffer>.GetFromMappedApiAsync(
                skyScannerResponse,
                Mapper,
                new
                {
                    Parameters = parameters,
                    AirlineLogos = _airlineLogo
                },
                cancellationToken);


            var cacheKey = GenerateCacheKey(parameters);
            SaveToBlobCacheAsync(cacheKey, result.RawResponse, result.UnifiedResponse);

            return result.UnifiedResponse;

        }
        catch (SkyScannerApiException ex)
        {
            Logger.LogError("SkyScanner API error: {statusCode}. {e}",
                ex.StatusCode, ex);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching for flights with Amadeus provider");
            throw;
        }
    }

    private void ExtractAirlineLogos(SkyScannerFlightOffer response)
    {
        foreach (var itinerary in response.Data.Itineraries)
        {
            foreach (var leg in itinerary.Legs)
            {
                foreach (var carrier in leg.Carriers.Marketing)
                {
                    var airlineCode = carrier.AlternateId ?? carrier.Name;
                    if (!string.IsNullOrEmpty(airlineCode) && !string.IsNullOrEmpty(carrier.LogoUrl))
                    {
                        _airlineLogo.TryAdd(airlineCode, carrier.LogoUrl);
                    }
                }
            }
        }
    }

    private void UpdateAirlineLogoCache()
    {
        if (!_airlineLogo.Any())
            return;

        var existingLogos = MemoryCache.GetItem<Dictionary<string, string>>(CacheEntry.AirlineLogoCache)
                            ?? new Dictionary<string, string>();

        foreach (var logo in _airlineLogo)
        {
            existingLogos[logo.Key] = logo.Value;
        }

        MemoryCache.Add(CacheEntry.AirlineLogoCache, existingLogos, TimeSpan.FromDays(30));

        Logger.LogInformation("Updated airline logo cache with {LogoCount} entries. Total cache size: {CacheSize}",
            _airlineLogo.Count, existingLogos.Count);
    }

    protected override string GenerateCacheKey(FlightSearchParameters parameters) => $"{ProviderName}_{parameters.Origin}_{parameters.Destination}";

    private readonly Func<SkyScannerFlightRequest, IDictionary<string, string>> _skyScannerMapper = criteria =>
    {
        var parameters = new Dictionary<string, string>
        {
            ["fromEntityId"] = criteria.FromEntityId,
            ["toEntityId"] = criteria.ToEntityId,
            ["departDate"] = criteria.DepartDate,
            ["returnDate"] = criteria.ReturnDate,
            ["market"] = "US",
            ["currency"] = "USD",
            ["stops"] = criteria.Stops ?? "direct,1stop",
            ["adults"] = criteria.Adults.ToString(),
            ["infants"] = criteria.Infants.ToString(),
            ["cabinClass"] = criteria.CabinClass ?? "economy",
            ["sort"] = "cheapest_price"
        };

        return parameters;
    };
}