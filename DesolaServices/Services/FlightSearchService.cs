using DesolaDomain.Aggregates;
using DesolaDomain.Interfaces;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Web;
using AutoMapper;
using System.Text;
using System.Text.Json;
using DesolaDomain.Model;

namespace DesolaServices.Services;

internal class FlightSearchService : IFlightSearchService
{
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<FlightSearchService> _logger;
    private readonly IAirportRepository _airportRepository;
    private readonly IAirlineRepository _airlineRepository;
    private List<Airport> _airports;

    public FlightSearchService(IApiService apiService, IConfiguration configuration, IMapper mapper, ILogger<FlightSearchService> logger, IAirportRepository airportRepository, IAirlineRepository airlineRepository)
    {
        _apiService = apiService;
        _configuration = configuration;
        _mapper = mapper;
        _logger = logger;
        _airportRepository = airportRepository;
        _airlineRepository = airlineRepository;
    }

    public async Task<Dictionary<string, FlightItineraryGroupResponse>> SearchFlightsAsync(FlightSearchBasicRequest criteria, CancellationToken cancellationToken)
    {
        try
        {

            _airports = await _airportRepository.GetAirportsAsync();

            ValidateAirportCode(criteria.Origin, criteria.Destination);
            var uri = BuildBasicFlightSearchUri(criteria);

            var accessToken = await _apiService.FetchAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Headers = { { "Authorization", $"{accessToken}" } }
            };

            var response = await _apiService.SendAsync<FlightOffer>(request, cancellationToken);

            var flightSearchResponse = await GroupItineraries(response, criteria.SortBy, criteria.SortOrder);

            return flightSearchResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching flights");
            throw;
        }
    }

    public async Task<Dictionary<string, FlightItineraryGroupResponse>> SearchAdvancedFlightsAsync(
        FlightSearchAdvancedRequest criteria, CancellationToken cancellationToken)
    {
        try
        {
            var uri = new Uri(_configuration["AmadeusApi_BaseUrl"] + "/v2/shopping/flight-offers");

            _airports = await _airportRepository.GetAirportsAsync();

            foreach (var originDestination in criteria.OriginDestinations)
            {
                ValidateAirportCode(originDestination.OriginLocationCode, originDestination.DestinationLocationCode);
            }


            var requestCriteria = JsonSerializer.Serialize(criteria);
            var requestContent = new StringContent(requestCriteria, Encoding.UTF8, "application/json");

            var accessToken = await _apiService.FetchAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = requestContent,
                Headers =
                {
                    { "Authorization", $"{accessToken}" },
                    { "X-HTTP-Method-Override", "GET" } // intentional override.
                    
                }
            };

            var response = await _apiService.SendAsync<FlightOffer>(request, cancellationToken);

            var flightSearchResponse = await GroupItineraries(response, criteria.SortBy, criteria.SortOrder);

            return flightSearchResponse;

            // return flightSearchResponse.Where(x => x.Value.Departure.NumberOfStopOver == criteria.MaxNumberOfStopOver).ToDictionary();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching advanced flights");
            throw;
        }
    }

    public async Task<Dictionary<string, FlightItineraryGroupResponse>> SearchSkyScannerFlightsAsync(SkyScannerFlightRequest criteria, CancellationToken cancellationToken)
    {
        try
        {
            // _airports = await _airportRepository.GetAirportsAsync();
            var uri = BuildSkyScannerFlightSearchUri(criteria);

            var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Headers =
                {
                    { "x-rapidapi-key", _configuration["RapidApiKey"] },
                    { "x-rapidapi-host", _configuration["RapidApiHost"] }
                }
            };

            var response = await _apiService.SendAsync<SkyScannerFlightOffer>(request, cancellationToken);
            var flightSearchResponse = GroupSkyScannerItineraries(response, criteria);

            return flightSearchResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching Sky scanner flights");
            throw;
        }
    }

    private void ValidateAirportCode(string origin, string destination)
    {
        if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
        {
            throw new ArgumentException("Origin and Destination must be specified");
        }

        if (_airports.FirstOrDefault(x => x.Code == origin) == null)
        {
            throw new ArgumentException("Invalid origin airport code", nameof(origin));
        }

        if (_airports.FirstOrDefault(x => x.Code == destination) == null)
        {
            throw new ArgumentException("Invalid destination airport code", nameof(destination));
        }
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
                    switch (legCount)
                    {
                        case 0:
                            departureItinerary = itineraryResponse;
                            break;
                        case 1:
                            returnItinerary = itineraryResponse;
                            break;
                    }
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

        return string.IsNullOrWhiteSpace(criteria.SortBy) && string.IsNullOrWhiteSpace(criteria.SortOrder) ? itineraries : ApplySorting(itineraries, criteria.SortBy, criteria.SortOrder);
    }

    private async Task<Dictionary<string, FlightItineraryGroupResponse>> GroupItineraries(FlightOffer flightOffer, string sortBy, string sortOrder)
    {
        var itineraries = new Dictionary<string, FlightItineraryGroupResponse>();
        var airlines = await _airlineRepository.GetAllAsync();

        var totalPrice = string.Empty;

        foreach (var data in flightOffer.Data)
        {
            FlightItineraryResponse departureItinerary = null;
            FlightItineraryResponse returnItinerary = null;

            foreach (var itinerary in data.Itineraries)
            {

                // modify the airline code to the airline name
                if (itinerary == null)
                {
                    continue;
                }

                itinerary.Segments.ForEach(x =>
                {
                    var airline = airlines.FirstOrDefault(a => a.IataCode == x.CarrierCode);
                    if (airline != null)
                    {
                        x.CarrierCode += $" - {airline.Name}";
                    }
                });


                var itineraryResponse = _mapper.Map<FlightItineraryResponse>(itinerary);
                totalPrice = data.Price.GrandTotal;

                if (departureItinerary == null)
                {
                    departureItinerary = itineraryResponse;
                }
                else
                {
                    returnItinerary = itineraryResponse;
                }
            }

            itineraries.Add(Guid.NewGuid().ToString(), new FlightItineraryGroupResponse
            {
                TotalPrice = decimal.Parse(totalPrice),
                PriceCurrency = "USD",
                Departure = departureItinerary,
                Return = returnItinerary
            });
        }

        return ApplySorting(itineraries, sortBy, sortOrder);
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

    private Uri BuildBasicFlightSearchUri(FlightSearchBasicRequest criteria)
    {
        var builder = new UriBuilder(_configuration["AmadeusApi_BaseUrl"] + "/v2/shopping/flight-offers");
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["originLocationCode"] = criteria.Origin;
        query["destinationLocationCode"] = criteria.Destination;
        query["departureDate"] = criteria.DepartureDate.ToString("yyyy-MM-dd");
        if (criteria.ReturnDate.HasValue)
        {
            query["returnDate"] = criteria.ReturnDate.Value.ToString("yyyy-MM-dd");
        }
        query["adults"] = criteria.Adults.ToString();
        query["max"] = criteria.MaxResults.ToString();
        builder.Query = query.ToString() ?? string.Empty;
        return builder.Uri;
    }

    private Uri BuildSkyScannerFlightSearchUri(SkyScannerFlightRequest criteria)
    {

        var url = $"{_configuration["SkyScannerUri"]}/{(criteria.IsOneWay ? "search-one-way" : "search-roundtrip")}";

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