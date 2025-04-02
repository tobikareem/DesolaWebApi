using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Interfaces;
using System.Web;
using AutoMapper;
using DesolaDomain.Settings;
using Microsoft.Extensions.Options;
using DesolaDomain.Entities.AmadeusFields.Basic;
using Microsoft.Extensions.Logging;
using Desola.Common.Exceptions;
using DesolaDomain.Entities.AmadeusFields.Response;

namespace DesolaInfrastructure.External.Amadeus;

public class AmadeusFlightProvider : IFlightProvider
{
    private readonly IApiService _apiService;
    private readonly AmadeusApi _amadeusConfig;
    private readonly ILogger<AmadeusFlightProvider> _logger;
    private readonly IMapper _mapper;

    public AmadeusFlightProvider(IApiService apiService, IOptions<AppSettings> settingsOptions, ILogger<AmadeusFlightProvider> logger, IMapper mapper)
    {
        _apiService = apiService;
        _logger = logger;
        _mapper = mapper;
        _amadeusConfig = settingsOptions.Value.ExternalApi.Amadeus;
    }

    public string ProviderName => _amadeusConfig.ProviderName;

    public async Task<UnifiedFlightSearchResponse> SearchFlightsAsync(FlightSearchParameters parameters, CancellationToken cancellationToken)
    {
        try
        {

            var uri = BuildBasicFlightSearchUri(parameters);
            var accessToken = await _apiService.FetchAccessTokenAsync(
                _amadeusConfig.TokenEndpointUrl,
                _amadeusConfig.ClientId,
                _amadeusConfig.ClientSecret,
                ProviderName);

            var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Headers = { { "Authorization", $"{accessToken}" } }
            };
            
            var flightOffers =  await _apiService.SendAsync<AmadeusFlightOffersResponse>(request, cancellationToken);

            var response = _mapper.Map<UnifiedFlightSearchResponse>(flightOffers.Data);

            return response;
        }
        catch (AmadeusApiException ex)
        {
            _logger.LogError("Amadeus API error: {StatusCode} - {ErrorTitle}: {ErrorDetail}",
                ex.StatusCode, ex.ErrorTitle, ex.ErrorDetail);
            throw; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for flights with Amadeus provider");
            throw;
        }

    }


    private Uri BuildBasicFlightSearchUri(FlightSearchParameters criteria)
    {
        var baseUrl = $"{_amadeusConfig.BaseUrl}/{_amadeusConfig.FlightSearchUrl}";
        var builder = new UriBuilder(baseUrl);
        var query = HttpUtility.ParseQueryString(string.Empty);

        query["originLocationCode"] = criteria.Origin;
        query["destinationLocationCode"] = criteria.Destination;
        query["departureDate"] = criteria.DepartureDate.ToString("yyyy-MM-dd");


        // Optionally include the return date for round-trip searches.
        if (criteria.ReturnDate.HasValue)
        {
            query["returnDate"] = criteria.ReturnDate.Value.ToString("yyyy-MM-dd");
        }

        query["adults"] = criteria.Adults.ToString();

        // Include children and infants if provided.
        if (criteria.Children > 0)
        {
            query["children"] = criteria.Children.ToString();
        }
        if (criteria.Infants > 0)
        {
            query["infants"] = criteria.Infants.ToString();
        }

        // Optionally specify travel class, airline filters, or nonstop preference.
        if (!string.IsNullOrEmpty(criteria.CabinClass))
        {
            query["travelClass"] = criteria.CabinClass;
        }
        if (criteria.IncludedAirlineCodes.Any())
        {
            query["includedAirlineCodes"] = string.Join(",", criteria.IncludedAirlineCodes);
        }
        if (criteria.ExcludedAirlineCodes.Any())
        {
            query["excludedAirlineCodes"] = string.Join(",", criteria.ExcludedAirlineCodes);
        }
        if (criteria.NonStop)
        {
            query["nonStop"] = "true";
        }

        query["currencyCode"] = criteria.CurrencyCode;

        if (criteria.MaxPrice > 0)
        {
            query["maxPrice"] = criteria.MaxPrice.ToString();
        }

        // Use the provided max value or fall back to a default of 250.
        // query["max"] = criteria.MaxPrice > 0 ? criteria.MaxPrice.ToString() : "250";

        builder.Query = query.ToString() ?? string.Empty;
        return builder.Uri;
    }


}