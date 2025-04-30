using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Interfaces;
using DesolaServices.Commands.Queries.FlightSearch;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using DesolaDomain.Settings;
using DesolaServices.Delegates;
using Microsoft.Extensions.Options;
using DesolaServices.Utility;
using DesolaDomain.Enums;

namespace DesolaServices.Handler.FlightSearch;

public class GetBasicFlightSearchQueryHandler : IRequestHandler<GetBasicFlightSearchQuery, Tuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>>
{
    private readonly Dictionary<string, FlightProviderDelegate> _flightProviderDelegates;
    private readonly ILogger<GetBasicFlightSearchQueryHandler> _logger;
    private readonly AppSettings _appSettings;
    private readonly ICacheService _cacheService;

    private readonly Dictionary<string, ProviderPerformanceStats> _providerStats = new();


    public GetBasicFlightSearchQueryHandler(ILogger<GetBasicFlightSearchQueryHandler> logger, IEnumerable<IFlightProvider> flightProviders, IOptions<AppSettings> settingsOptions, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _flightProviderDelegates = flightProviders.ToDictionary(provider => provider.ProviderName, provider => new FlightProviderDelegate(provider.SearchFlightsAsync));
        _appSettings = settingsOptions.Value;
    }
    public async Task<Tuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>> Handle(GetBasicFlightSearchQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting flight request from {request.SearchParameters.Origin} to {request.SearchParameters.Destination}");

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request.SearchParameters);

        var errors = new Dictionary<string, string[]>();

        if (!Validator.TryValidateObject(request.SearchParameters, validationContext, validationResults, true))
        {
            errors = validationResults
                .GroupBy(vr => string.Join(", ", vr.MemberNames))
                .ToDictionary(
                    g => string.IsNullOrEmpty(g.Key) ? "General" : g.Key,
                    g => g.Select(vr => vr.ErrorMessage).ToArray()
                );

            if (errors.Count > 0)
            {
                return new Tuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>(new UnifiedFlightSearchResponse(), errors);
            }

        }

        var providersToCall = new[]
        { 
            _appSettings.ExternalApi.Amadeus.ProviderName,
            _appSettings.ExternalApi.RapidApi.SkyScannerProviderName
        };

        var tasks = providersToCall.Select(providerName =>
        {
            if (_flightProviderDelegates.TryGetValue(providerName, out var providerDelegate))
            {

                return TimeOutAndRetry.ExecuteAsync(
                    providerName, 
                    providerDelegate,
                    _providerStats,
                    request.SearchParameters, 
                    cancellationToken, 
                    _logger,
                    TimeSpan.FromSeconds(60));

            }
            _logger.LogWarning($"Provider {providerName} not found.");

            return Task.FromResult<UnifiedFlightSearchResponse>(null);
        }).ToList();


        var results = await Task.WhenAll(tasks);

        foreach (var (provider, stats) in _providerStats)
        {
            _logger.LogInformation($"Stats for {provider}: " +
                                   $"Success={stats.SuccessCount}, " +
                                   $"Failure={stats.FailureCount}, " +
                                   $"Average Response={stats.AverageResponseTimeMs:F2}ms");
        }


        var successfulResponses = results.Where(r => r != null).ToList();

        var airlineLogoDictionary = _cacheService.GetItem<Dictionary<string, string>>(CacheEntry.AirlineLogoCache) ?? new Dictionary<string, string>();
        var unifiedResponse = CombineFlightResponses(successfulResponses, airlineLogoDictionary);

        return new Tuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>(unifiedResponse, errors);
    }
    
    private static UnifiedFlightSearchResponse CombineFlightResponses(
        IReadOnlyCollection<UnifiedFlightSearchResponse> successfulResponses,
        IReadOnlyDictionary<string, string> airlineLogoDictionary)
    {

        if (!successfulResponses.Any())
        {
            return new UnifiedFlightSearchResponse { Offers = new List<UnifiedFlightOffer>() };
        }

        var allOffers = successfulResponses.SelectMany(r => r.Offers).ToList();

        var groupOffers = allOffers.GroupBy(BuildOfferGroupingKey);

        var bestOffers = new List<UnifiedFlightOffer>();

        foreach (var group in groupOffers)
        {
            var cheapestOffer = group.OrderBy(o => o.TotalPrice).First();
            
            // TODO: Re-evaluate the complexity of finding missing values
            RepairMissingAirlineLogo(cheapestOffer, airlineLogoDictionary);

            bestOffers.Add(cheapestOffer);
        }

        return new UnifiedFlightSearchResponse
        {
            Offers = bestOffers,
            TotalResults = bestOffers.Count,
            CurrencyCode = bestOffers.FirstOrDefault()?.FormattedPrice?.Split(' ').FirstOrDefault() ?? "USD"
        };
    }

    private static void RepairMissingAirlineLogo(UnifiedFlightOffer offer, IReadOnlyDictionary<string, string> logoDictionary)
    {
        foreach (var itinerary in offer.Itineraries ?? Enumerable.Empty<UnifiedItinerary>())
        {
            foreach (var segment in itinerary.Segments ?? Enumerable.Empty<UnifiedSegment>())
            {
                if (!string.IsNullOrEmpty(segment.AirlineLogo)) continue;

                if (logoDictionary == null || !logoDictionary.TryGetValue(segment.MarketingAirline, out var logo)) 
                    continue;

                var logoValue = logo;
                segment.AirlineLogo = logoValue;
            }
        }
    }

    private static string BuildOfferGroupingKey(UnifiedFlightOffer offer)
    {
        if (offer.Itineraries == null || !offer.Itineraries.Any())
            return string.Empty;

        var outbound = offer.Itineraries.FirstOrDefault(i => i.Direction == "Outbound");
        var inbound = offer.Itineraries.FirstOrDefault(i => i.Direction == "Return");

        var outboundKey = outbound?.Segments.FirstOrDefault() is { } outboundSegment
            ? $"{outboundSegment.Departure?.AirportCode}-{outboundSegment.Arrival?.AirportCode}"
            : "";

        var inboundKey = inbound?.Segments.FirstOrDefault() is { } inboundSegment
            ? $"{inboundSegment.Departure?.AirportCode}-{inboundSegment.Arrival?.AirportCode}"
            : "";

        return $"{outboundKey}|{inboundKey}";
    }
}