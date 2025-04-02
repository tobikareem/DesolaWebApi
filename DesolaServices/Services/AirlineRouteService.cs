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
    private readonly AmadeusApi _amadeusConfig;

    public AirlineRouteService(IOptions<AppSettings> configuration, IApiService apiService, IAirlineRepository airlineRepository, IMapper mapper, IOptions<AppSettings> settingsOptions)
    {
        _configuration = configuration.Value;
        _apiService = apiService;
        _airlineRepository = airlineRepository;
        _mapper = mapper;
        _amadeusConfig = settingsOptions.Value.ExternalApi.Amadeus;
    }

    public async Task<List<FlightRouteResponse>> GetAirportRoutesAsync(string airlineCode, int max, string countyCode, CancellationToken cancellationToken)
    {

        var airlineWithCode = await _airlineRepository.GetByCodeAsync(airlineCode);

        if (airlineWithCode == null)
        {
            throw new InvalidOperationException("Airline not found");
        }

        var uri = BuildSearchUri(airlineCode, countyCode, max);

        var accessToken = await _apiService.FetchAccessTokenAsync(_amadeusConfig.TokenEndpointUrl, _amadeusConfig.ClientId, _amadeusConfig.ClientSecret, _amadeusConfig.ProviderName);

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