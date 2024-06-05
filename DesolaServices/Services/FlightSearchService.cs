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

namespace DesolaServices.Services;

internal class FlightSearchService : IFlightSearchService
{
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger<FlightSearchService> _logger;

    public FlightSearchService(IApiService apiService, IConfiguration configuration, IMapper mapper, ILogger<FlightSearchService> logger)
    {
        _apiService = apiService;
        _configuration = configuration;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Dictionary<string, FlightItineraryGroupResponse>> SearchFlightsAsync(FlightSearchBasicRequest criteria)
    {
        try
        {
            ValidateCriteria(criteria);
            var uri = BuildSearchUri(criteria);

            var accessToken = await _apiService.FetchAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Headers = { { "Authorization", $"{accessToken}" } }
            };

            var response = await _apiService.SendAsync<FlightOffer>(request);

            var flightSearchResponse = GroupItineraries(response, criteria);

            return flightSearchResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching flights");
            throw;
        }
    }

    private void ValidateCriteria(FlightSearchBasicRequest criteria)
    {
        if (string.IsNullOrEmpty(criteria.Origin) || string.IsNullOrEmpty(criteria.Destination))
        {
            throw new ArgumentException("Origin and Destination must be specified");
        }
    }

    private Dictionary<string, FlightItineraryGroupResponse> GroupItineraries(FlightOffer flightOffer, FlightSearchBasicRequest criteriaBasicRequest)
    {
        var itineraries = new Dictionary<string, FlightItineraryGroupResponse>();

        foreach (var data in flightOffer.Data)
        {
            FlightItineraryResponse departureItinerary = null;
            FlightItineraryResponse returnItinerary = null;

            foreach (var itinerary in data.Itineraries)
            {
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

        return ApplySorting(itineraries, criteriaBasicRequest.SortBy, criteriaBasicRequest.SortOrder);
    }

    private Dictionary<string, FlightItineraryGroupResponse> ApplySorting(Dictionary<string, FlightItineraryGroupResponse> itineraries, string sortBy, string sortOrder)
    {
        var sortedItineraries = itineraries.AsQueryable();

        switch (sortBy?.ToLower())
        {
            case "price":
                sortedItineraries = sortOrder?.ToLower() == "desc"
                    ? sortedItineraries.OrderByDescending(x => x.Value.Departure.TotalPrice).ThenByDescending(x => x.Value.Return.TotalPrice)
                    : sortedItineraries.OrderBy(x => x.Value.Departure.TotalPrice).ThenBy(x => x.Value.Return.TotalPrice);
                break;
            case "duration":
                sortedItineraries = sortOrder?.ToLower() == "desc"
                    ? sortedItineraries.OrderByDescending(x => x.Value.Departure.TotalDuration).ThenByDescending(x => x.Value.Return.TotalDuration)
                    : sortedItineraries.OrderBy(x => x.Value.Departure.TotalDuration).ThenBy(x => x.Value.Return.TotalDuration);
                break;
            default:
                _logger.LogWarning("Invalid sort by criteria. Sorting by price in ascending order");
                break;
        }

        return sortedItineraries.ToDictionary(x => x.Key, x => x.Value);

    }

    public async Task<FlightOffer> SearchAdvancedFlightsAsync(FlightSearchAdvancedRequest criteria)
    {
        try
        {
            var uri = new Uri(_configuration["AmadeusApi_BaseUrl"] + "/v2/shopping/flight-offers");
            var accessToken = await _apiService.FetchAccessTokenAsync();

            var requestContent = new StringContent(JsonSerializer.Serialize(criteria), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = requestContent,
                Headers =
                {
                    { "Authorization", $"{accessToken}" },
                    { "X-HTTP-Method-Override", "GET" }
                }
            };

            return await _apiService.SendAsync<FlightOffer>(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching advanced flights");
            throw;
        }
    }

    private Uri BuildSearchUri(FlightSearchBasicRequest criteria)
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