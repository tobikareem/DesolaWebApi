using AutoMapper;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Web;
using DesolaDomain.Entities.SkyScannerFields;
using DesolaDomain.Aggregates;
using Desola.Common.Exceptions;
using DesolaDomain.Enums;

namespace DesolaInfrastructure.External.SkyScanner;

public class SkyScannerFlightProvider : IFlightProvider
{
    private readonly ILogger<SkyScannerFlightProvider> _logger;
    private readonly RapidApi _skyScannerConfig;
    private readonly IMapper _mapper;
    private readonly IApiService _apiService;

    private readonly ICacheService _memoryCache;
    private readonly Dictionary<string, string> _airlineLogo;
    public string ProviderName => _skyScannerConfig.SkyScannerProviderName;

    public SkyScannerFlightProvider(ILogger<SkyScannerFlightProvider> logger, IOptions<AppSettings> settingsOptions, IMapper mapper, IApiService apiService, ICacheService memoryCache)
    {
        _logger = logger;
        _skyScannerConfig = settingsOptions.Value.ExternalApi.RapidApi;
        _mapper = mapper;
        _airlineLogo = new Dictionary<string, string>();
        _apiService = apiService;
        _memoryCache = memoryCache;
    }
    public async Task<UnifiedFlightSearchResponse> SearchFlightsAsync(FlightSearchParameters parameters, CancellationToken cancellationToken)
    {
        try
        {
            var skyScannerRequest = _mapper.Map<SkyScannerFlightRequest>(parameters);

            var uri = BuildSkyScannerFlightSearchUri(skyScannerRequest);

            var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Headers = {
                    { "x-rapidapi-key", _skyScannerConfig.SkyScannerKey },
                    { "x-rapidapi-host", _skyScannerConfig.SkyScannerHost}
                }
            };

            var cacheKey = $"{CacheEntry.SkyScannerFlightSearchCache}_{skyScannerRequest.FromEntityId}_{skyScannerRequest.ToEntityId}_{skyScannerRequest.DepartDate}_{skyScannerRequest.ReturnDate}";

            if(_memoryCache.Contains(cacheKey))
            {
                var cachedResponse = _memoryCache.GetItem<UnifiedFlightSearchResponse>(cacheKey);
                if (cachedResponse != null)
                {
                    _logger.LogInformation($"Returning cached response for SkyScanner flight search: {cacheKey}");
                    return cachedResponse;
                }
            }

            var flightOffers = await _apiService.SendAsync<SkyScannerFlightOffer>(request, cancellationToken);

            var groupedItineraries = GroupSkyScannerItineraries(flightOffers, new SkyScannerFlightRequest());

            var offers = groupedItineraries.Select(kvp => new UnifiedFlightOffer
            {
                Id = kvp.Key,
                Provider = "SkyScanner",
                FlightSource = "SkyScanner",
                TotalPrice = kvp.Value.TotalPrice,
                FormattedPrice = $"{kvp.Value.PriceCurrency} {kvp.Value.TotalPrice}",
                Itineraries = BuildUnifiedItineraries(kvp.Value),
                IsRefundable = false,
                BaggageAllowance = null,
                FareConditions = new List<string>(),
                AvailableSeats = 0,
                ValidatingCarrier = kvp.Value.Departure.Segments.FirstOrDefault()?.FlightNumber.Split(" ")[0],
                LastTicketingDate = null
            }).ToList();

            var finalResponse = new UnifiedFlightSearchResponse
            {
                Offers = offers,
                TotalResults = offers.Count,
                CurrencyCode = "USD"
            };

            // set to cache
            _memoryCache.Add(cacheKey, finalResponse, TimeSpan.FromHours(10));
            return finalResponse;


        }
        catch (AmadeusApiException ex)
        {
            _logger.LogError("SkyScanner API error: {StatusCode} - {ErrorTitle}: {ErrorDetail}",
                ex.StatusCode, ex.ErrorTitle, ex.ErrorDetail);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for flights with Amadeus provider");
            throw;
        }
    }

    private IEnumerable<UnifiedItinerary> BuildUnifiedItineraries(FlightItineraryGroupResponse group)
    {
        var itineraries = new List<UnifiedItinerary>();

        if (group.Departure != null)
        {
            itineraries.Add(new UnifiedItinerary
            {
                Direction = "Outbound",
                Duration = TimeSpan.Parse(group.Departure.TotalDuration),
                FormattedDuration = group.Departure.TotalDuration,
                Stops = group.Departure.NumberOfStopOver,
                Segments = group.Departure.Segments.Select(segment => _mapper.Map<UnifiedSegment>(segment)).ToList()
            });
        }

        if (group.Return != null)
        {
            itineraries.Add(new UnifiedItinerary
            {
                Direction = "Return",
                Duration = TimeSpan.Parse(group.Return.TotalDuration),
                FormattedDuration = group.Return.TotalDuration,
                Stops = group.Return.NumberOfStopOver,
                Segments = group.Return.Segments.Select(segment => _mapper.Map<UnifiedSegment>(segment)).ToList()
            });
        }

        return itineraries;
    }


    private Dictionary<string, FlightItineraryGroupResponse> GroupSkyScannerItineraries(SkyScannerFlightOffer flightOffer, SkyScannerFlightRequest criteria)
    {
        var itineraries = new Dictionary<string, FlightItineraryGroupResponse>();

        foreach (var data in flightOffer.Data.Itineraries)
        {
            FlightItineraryResponse departureItinerary = null;
            FlightItineraryResponse returnItinerary = null;

            var itineraryId = data.Id.Split('|', StringSplitOptions.RemoveEmptyEntries);

            var legCount = 0;
            foreach (var leg in data.Legs)
            {
                var segmentList = new List<FlightSegmentResponse>();

                foreach (var legSegment in leg.Segments)
                {
                    var segments = _mapper.Map<FlightSegmentResponse>(legSegment);

                    if (leg.Carriers.Marketing.Any())
                    {
                        var carrier = leg.Carriers.Marketing.First();
                        segments.AircraftPhotoLink = carrier.LogoUrl;
                        segments.Airline = carrier.Name;

                        var airlineCode = carrier.AlternateId ?? carrier.Name;
                        if (!string.IsNullOrEmpty(airlineCode))
                        {
                            _airlineLogo.TryAdd(airlineCode, segments.AircraftPhotoLink);
                        }
                    }

                    segmentList.Add(segments);
                }

                var itineraryResponse = new FlightItineraryResponse
                {
                    Segments = segmentList,
                    TotalDuration = TimeSpan.FromMinutes(leg.DurationInMinutes).ToString(@"hh\:mm"),
                    NumberOfStopOver = leg.StopCount
                };


                if (legCount < itineraryId.Length && leg.Id == itineraryId[legCount])
                {
                    if (legCount == 0) departureItinerary = itineraryResponse;
                    if (legCount == 1) returnItinerary = itineraryResponse;
                }

                legCount++;
            }

            itineraries.Add(data.Id, new FlightItineraryGroupResponse
            {
                TotalPrice = data.Price.Raw,
                PriceCurrency = "USD",
                Departure = departureItinerary,
                Return = returnItinerary
            });

        }

        if (!_airlineLogo.Any())
            return string.IsNullOrWhiteSpace(criteria.SortBy) && string.IsNullOrWhiteSpace(criteria.SortOrder)
                ? itineraries
                : ApplySorting(itineraries, criteria.SortBy, criteria.SortOrder);

        var existingLogos = _memoryCache.GetItem<Dictionary<string, string>>(CacheEntry.AirlineLogoCache)
                            ?? new Dictionary<string, string>();
            
        foreach (var logo in _airlineLogo)
        {
            existingLogos[logo.Key] = logo.Value;
        }
            
        _memoryCache.Add(CacheEntry.AirlineLogoCache, existingLogos, TimeSpan.FromDays(30));
            
        _logger.LogInformation($"Updated airline logo cache with {_airlineLogo.Count} entries. Total cache size: {existingLogos.Count}");
        return string.IsNullOrWhiteSpace(criteria.SortBy) && string.IsNullOrWhiteSpace(criteria.SortOrder) ? itineraries : ApplySorting(itineraries, criteria.SortBy, criteria.SortOrder);

    }

    private Dictionary<string, FlightItineraryGroupResponse> ApplySorting(Dictionary<string, FlightItineraryGroupResponse> itineraries, string sortBy, string sortOrder)
    {
        var sortedItineraries = itineraries.AsEnumerable();

        switch (sortBy?.ToLower())
        {
            case "price":
                sortedItineraries = sortOrder?.ToLower() == "desc"
                    ? sortedItineraries.OrderByDescending(x => x.Value.TotalPrice)
                    : sortedItineraries.OrderBy(x => x.Value.TotalPrice);

                break;
            case "duration":
                sortedItineraries = sortOrder?.ToLower() == "desc"
                    ? sortedItineraries.OrderByDescending(x => x.Value.Departure.TotalDuration)
                    : sortedItineraries.OrderBy(x => x.Value.Departure.TotalDuration);
                break;
            default:
                _logger.LogWarning("Invalid sort by criteria. Sorting by price in ascending order");
                sortedItineraries = sortedItineraries.OrderBy(x => x.Value.TotalPrice);
                break;
        }

        return sortedItineraries.ToDictionary(x => x.Key, x => x.Value);

    }
    private Uri BuildSkyScannerFlightSearchUri(SkyScannerFlightRequest criteria)
    {

        var url = $"{_skyScannerConfig.SkyScannerUri}/{(criteria.IsOneWay ? "search-one-way" : "search-roundtrip")}";

        var builder = new UriBuilder(url);
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["fromEntityId"] = criteria.FromEntityId;
        query["toEntityId"] = criteria.ToEntityId;
        query["departDate"] = criteria.DepartDate;
        query["returnDate"] = criteria.ReturnDate;
        query["market"] = "US";
        query["currency"] = "USD";
        query["stops"] = criteria.Stops ?? "direct,1stop";
        query["adults"] = criteria.Adults.ToString();
        query["infants"] = criteria.Infants.ToString();
        query["cabinClass"] = criteria.CabinClass ?? "economy";
        builder.Query = query.ToString() ?? string.Empty;
        return builder.Uri;
    }
}