using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.FlightSearch;
using DesolaServices.Delegates;
using DesolaServices.Handler.FlightSearch;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Utility;

public class TimeOutAndRetry
{

    public static async Task<UnifiedFlightSearchResponse> ExecuteAsync(
        string providerName,
        FlightProviderDelegate providerDelegate,
        Dictionary<string, ProviderPerformanceStats> providerStats,
        FlightSearchParameters parameters,
        CancellationToken cancellationToken,
        ILogger<GetBasicFlightSearchQueryHandler> logger,
        TimeSpan timeoutMilliseconds, int maxRetries = 2)
    {
        var attempt = 0;

        while (true)
        {
            attempt++;

            using var timeoutCts = new CancellationTokenSource(timeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                logger.LogInformation($"Attempt {attempt}: Calling provider {providerName}");

                var result = await providerDelegate(parameters, linkedCts.Token);

                stopwatch.Stop();

                RecordProviderSuccess(providerName, stopwatch.Elapsed, providerStats);

                logger.LogInformation($"Provider {providerName} returned successfully.");

                return result;
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {

                stopwatch.Stop();
                logger.LogWarning($"Provider {providerName} timed out after {timeoutMilliseconds}ms.");
                RecordProviderFailure(providerName, providerStats);
            }
            catch (Exception ex)
            {

                stopwatch.Stop();
                logger.LogError(ex, $"Provider {providerName} failed on attempt {attempt}.");
                RecordProviderFailure(providerName, providerStats);
            }

            if (attempt <= maxRetries) continue;

            logger.LogError($"Provider {providerName} failed after {maxRetries} retries.");
            return null;
        }
    }

    private static void RecordProviderFailure(string providerName, Dictionary<string, ProviderPerformanceStats> providerStats)
    {
        if (!providerStats.TryGetValue(providerName, out var stats))
        {
            stats = new ProviderPerformanceStats();
            providerStats[providerName] = stats;
        }

        stats.RecordFailure();
    }

    private static void RecordProviderSuccess(string providerName, TimeSpan responseTime, IDictionary<string, ProviderPerformanceStats> providerStats)
    {
        if (!providerStats.TryGetValue(providerName, out var stats))
        {
            stats = new ProviderPerformanceStats();
            providerStats[providerName] = stats;
        }

        stats.RecordSuccess(responseTime);
    }
}
