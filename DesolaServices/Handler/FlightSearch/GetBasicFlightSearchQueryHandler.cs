using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Interfaces;
using DesolaServices.Commands.Queries.FlightSearch;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Settings;
using DesolaServices.Delegates;
using Microsoft.Extensions.Options;
using DesolaServices.Utility;
using DesolaServices.Services;

namespace DesolaServices.Handler.FlightSearch;

public class GetBasicFlightSearchQueryHandler : IRequestHandler<GetBasicFlightSearchQuery, ValueTuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>>
{
    private readonly Dictionary<string, FlightProviderDelegate> _flightProviderDelegates;
    private readonly ILogger<GetBasicFlightSearchQueryHandler> _logger;
    private readonly AppSettings _appSettings;

    private readonly Dictionary<string, ProviderPerformanceStats> _providerStats = new();

    public GetBasicFlightSearchQueryHandler(ILogger<GetBasicFlightSearchQueryHandler> logger, IEnumerable<IFlightProvider> flightProviders, IOptions<AppSettings> settingsOptions)
    {
        _logger = logger;
        _flightProviderDelegates = flightProviders.ToDictionary(provider => provider.ProviderName, provider => new FlightProviderDelegate(provider.SearchFlightsAsync));
        _appSettings = settingsOptions.Value;
    }
    public async Task<ValueTuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>> Handle(GetBasicFlightSearchQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting flight request from {request.SearchParameters.Origin} to {request.SearchParameters.Destination}");

        var errors = ValidateRequest(request.SearchParameters);

        if (errors.Count > 0)
        {
            return new ValueTuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>(new UnifiedFlightSearchResponse(), errors);
        }

        var providersToCall = new[]
        {
              _appSettings.ExternalApi.Amadeus.ProviderName,
             _appSettings.ExternalApi.RapidApi.SkyScannerProviderName,
            // _appSettings.ExternalApi.RapidApi.GoogleProviderName
        };

        var tasks = providersToCall.Select(providerName =>
        {
            if (_flightProviderDelegates.TryGetValue(providerName, out var providerDelegate))
            {

                return TimeOutAndRetry.ExecuteAsync(
                    providerName,
                    providerDelegate,
                    _providerStats,
                    request.SearchParameters,
                    cancellationToken,
                    _logger,
                    TimeSpan.FromSeconds(60));

            }
            _logger.LogWarning($"Provider {providerName} not found.");

            return Task.FromResult<UnifiedFlightSearchResponse>(null);
        });

        var results = await Task.WhenAll(tasks);

        LogPerformanceStats();

        var successfulResponses = results.Where(r =>
        {
            PostProcessUnifiedResponse(r, request.SearchParameters);
            return r != null;
        }).ToList();

        var unifiedResponse = FlightResultAggregator.CombineResults(successfulResponses, request.SearchParameters.SortBy, request.SearchParameters.SortOrder);

        return new ValueTuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>(unifiedResponse, errors);
    }

    private static void PostProcessUnifiedResponse(UnifiedFlightSearchResponse response, FlightSearchParameters parameters)
    {
        response.Origin = parameters.Origin;
        response.Destination = parameters.Destination;
        response.DepartureDate = parameters.DepartureDate;
        response.ReturnDate = parameters.ReturnDate;

        // Ensure we have some basic offer properties set
        if (response.Offers == null) return;

        foreach (var offer in response.Offers)
        {
            // offer.Provider ??= response.;
            // offer.FlightSource ??= ProviderName;

            // Ensure baggage allowance is set
            offer.BaggageAllowance ??= new BaggageAllowance
            {
                CheckedBags = 0,
                Description = "No checked baggage information available"
            };

            // Ensure fare conditions are set
            if (offer.FareConditions == null || !offer.FareConditions.Any())
            {
                offer.FareConditions = new List<string> { "Fare conditions not specified" };
            }
        }
    }

    private static Dictionary<string, string[]> ValidateRequest(FlightSearchParameters parameters)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(parameters);

        var errors = new Dictionary<string, string[]>();

        if (!Validator.TryValidateObject(parameters, validationContext, validationResults, true))
        {
            errors = validationResults
                .GroupBy(vr => string.Join(", ", vr.MemberNames))
                .ToDictionary(
                    g => string.IsNullOrEmpty(g.Key) ? "General" : g.Key,
                    g => g.Select(vr => vr.ErrorMessage).ToArray()
                );

        }

        return errors;
    }

    private void LogPerformanceStats()
    {
        foreach (var (provider, stats) in _providerStats)
        {
            _logger.LogInformation($"Stats for {provider}: " +
                                   $"Success={stats.SuccessCount}, " +
                                   $"Failure={stats.FailureCount}, " +
                                   $"Average Response={stats.AverageResponseTimeMs:F2}ms");
        }
    }
}