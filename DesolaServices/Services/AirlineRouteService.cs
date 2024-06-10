
using amadeus.resources;
using DesolaServices.Interfaces;
using System.Web;
using AutoMapper;
using DesolaDomain.Interfaces;
using Microsoft.Extensions.Configuration;
using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Services;

public class AirlineRouteService : IAirlineRouteService
{
    private readonly IConfiguration _configuration;
    private readonly IApiService _apiService;
    private readonly IAirlineRepository _airlineRepository;
    private readonly IMapper _mapper;

    public AirlineRouteService(IConfiguration configuration, IApiService apiService, IAirlineRepository airlineRepository, IMapper mapper)
    {
        _configuration = configuration;
        _apiService = apiService;
        _airlineRepository = airlineRepository;
        _mapper = mapper;
    }

    public async Task<List<FlightRouteResponse>> GetAirportRoutesAsync(string airlineCode, int max)
    {

        await ValidateAirlineCodeAsync(airlineCode);

        var uri = BuildSearchUri(airlineCode, max);

        var accessToken = await _apiService.FetchAccessTokenAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, uri)
        {
            Headers = { { "Authorization", $"{accessToken}" } }
        };

        var airportRoute = await _apiService.SendAsync<AirportRoute>(request);

        var response = _mapper.Map<List<FlightRouteResponse>>(airportRoute.Data);

        return response;
    }

    private async Task ValidateAirlineCodeAsync(string airlineCode)
    {
        var airlineCodes = await _airlineRepository.GetAllAirlineIataCodesAsync();

        if (!airlineCodes.Contains(airlineCode))
        {
            throw new ArgumentException("Invalid airline code", nameof(airlineCode));
        }
    }

    private Uri BuildSearchUri(string airlineCode, int max = 20)
    {
        var builder = new UriBuilder(_configuration["AmadeusApi_BaseUrl"] + "/v1/airline/destinations");
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["airlineCode"] = airlineCode;
        query["max"] = max.ToString();
        builder.Query = query.ToString() ?? string.Empty;
        return builder.Uri;
    }
}