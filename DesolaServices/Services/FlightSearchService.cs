
using System.Text;
using System.Text.Json;
using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.Interfaces;
using System.Web;
using AutoMapper;
using DesolaDomain.Interfaces;
using DesolaServices.DataTransferObjects.Responses;
using Microsoft.Extensions.Configuration;

namespace DesolaServices.Services;

internal class FlightSearchService : IFlightSearchService
{
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;
    private readonly IAmadeusService _amadeusService;
    private readonly IMapper _mapper;

    public FlightSearchService(IApiService apiService, IConfiguration configuration, IAmadeusService amadeusService, IMapper mapper)
    {
        _apiService = apiService;
        _configuration = configuration;
        _amadeusService = amadeusService;
        _mapper = mapper;
    }
    public async Task<FlightOffer> SearchFlightsAsync(FlightSearchBasic criteria)
    {
        var uri = BuildSearchUri(criteria);
        var accessToken = await _apiService.FetchAccessTokenAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, uri)
        {
            Headers = {
                { "Authorization", $"{accessToken}" }
            }
        };

        var response = await _apiService.SendAsync<FlightOffer>(request);

        SearchFlightWithLegs(response);

        return response;
    }

    public void SearchFlightWithLegs(FlightOffer flightOffer)
    {
        var flightSearchResponse = _mapper.Map<FlightSearchResponse>(flightOffer);

    }


    public async Task<FlightOffer> SearchAdvancedFlightsAsync(FlightSearchAdvanced criteria)
    {
        var uri = new Uri(_configuration["AmadeusApi_BaseUrl"] + "/v2/shopping/flight-offers");
        var accessToken = await _apiService.FetchAccessTokenAsync();

        var requestContent = new StringContent(JsonSerializer.Serialize(criteria), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = requestContent,
            Headers =
            {
                { "Authorization", $"Bearer {accessToken}" },
                { "X-HTTP-Method-Override", "GET" }
            }
        };

        var response = await _apiService.SendAsync<FlightOffer>(request);
        return response;
    }


    private Uri BuildSearchUri(FlightSearchBasic criteria)
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