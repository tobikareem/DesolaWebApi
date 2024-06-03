
using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.Interfaces;
using System.Web;
using DesolaDomain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DesolaServices.Services;

internal class FlightSearchService : IFlightSearchService
{
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;
    private readonly IAmadeusService _amadeusService;
    

    public FlightSearchService(IApiService apiService, IConfiguration configuration, IAmadeusService amadeusService)
    {
        _apiService = apiService;
        _configuration = configuration;
        _amadeusService = amadeusService;
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

        return response;
    }

    public async Task<amadeus.resources.FlightOffer[]> SearchAdvancedFlightsAsync(FlightSearchAdvanced criteria)
    {
       throw new NotImplementedException();
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
        builder.Query = query.ToString();
        return builder.Uri;
    }
}