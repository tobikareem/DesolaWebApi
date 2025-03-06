using DesolaServices.Interfaces;
using System.Web;
using AutoMapper;
using DesolaDomain.Interfaces;
using DesolaDomain.Aggregates;
using DesolaDomain.Settings;
using DesolaServices.DataTransferObjects.Responses;
using Microsoft.Extensions.Options;

namespace DesolaServices.Services;

public class AirlineRouteService : IAirlineRouteService
{
    private readonly AppSettings _configuration;
    private readonly IApiService _apiService;
    private readonly IAirlineRepository _airlineRepository;
    private readonly IMapper _mapper;

    public AirlineRouteService(IOptions<AppSettings> configuration, IApiService apiService, IAirlineRepository airlineRepository, IMapper mapper)
    {
        _configuration = configuration.Value;
        _apiService = apiService;
        _airlineRepository = airlineRepository;
        _mapper = mapper;
    }

    public async Task<List<FlightRouteResponse>> GetAirportRoutesAsync(string airlineCode, int max, string countyCode, CancellationToken cancellationToken)
    {

        await ValidateAirlineCodeAsync(airlineCode);

        var uri = BuildSearchUri(airlineCode, countyCode, max);

        var accessToken = await _apiService.FetchAccessTokenAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, uri)
        {
            Headers = { { "Authorization", $"{accessToken}" } }
        };

        var airportRoute = await _apiService.SendAsync<AirportRoute>(request, cancellationToken);
        var result = airportRoute.Data;

        //if (countyCode != "all")
        //{
        //    result = result.Where(x => x.Address.CountryCode.ToLowerInvariant() == countyCode.ToLowerInvariant()).ToList();
        //}

        var response = _mapper.Map<List<FlightRouteResponse>>(result);

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

    private Uri BuildSearchUri(string airlineCode, string arrivalCountryCode, int max = 100)
    {
        var builder = new UriBuilder(_configuration.ExternalApi.Amadeus.BaseUrl + "/v1/airline/destinations");
        var query = HttpUtility.ParseQueryString(string.Empty);

        query[nameof(airlineCode)] = airlineCode;
        query[nameof(max)] = max.ToString();

        if (!string.IsNullOrWhiteSpace(arrivalCountryCode))
        {
            query[nameof(arrivalCountryCode)] = arrivalCountryCode;
        }

        builder.Query = query.ToString() ?? string.Empty;
        return builder.Uri;
    }
}