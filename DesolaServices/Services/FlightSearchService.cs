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

    public async Task<Dictionary<string, FlightItineraryGroupResponse>> SearchFlightsAsync(FlightSearchBasicRequest criteria)
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

            var response = await _apiService.SendAsync<FlightOffer>(request);

            var flightSearchResponse = await GroupItineraries(response, criteria.SortBy, criteria.SortOrder);

            return flightSearchResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching flights");
            throw;
        }
    }
    public async Task<Dictionary<string, FlightItineraryGroupResponse>> SearchAdvancedFlightsAsync(FlightSearchAdvancedRequest criteria)
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

            var response = await _apiService.SendAsync<FlightOffer>(request);

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

    private async Task<Dictionary<string, FlightItineraryGroupResponse>> GroupItineraries(FlightOffer flightOffer, string sortBy, string sortOrder)
    {
        var itineraries = new Dictionary<string, FlightItineraryGroupResponse>();
        var airlines = await _airlineRepository.GetAllAirlinesAsync();

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
                itineraryResponse.TotalPrice = decimal.Parse(data.Price.GrandTotal);
                itineraryResponse.PriceCurrency = data.Price.Currency;

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
                    ? sortedItineraries.OrderByDescending(x => x.Value.Departure.TotalPrice)
                    : sortedItineraries.OrderBy(x => x.Value.Departure.TotalPrice);

                break;
            case "duration":
                sortedItineraries = sortOrder?.ToLower() == "desc"
                    ? sortedItineraries.OrderByDescending(x => x.Value.Departure.TotalDuration)
                    : sortedItineraries.OrderBy(x => x.Value.Departure.TotalDuration);
                break;
            default:
                _logger.LogWarning("Invalid sort by criteria. Sorting by price in ascending order");
                sortedItineraries = sortedItineraries.OrderBy(x => x.Value.Departure.TotalPrice);
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
}