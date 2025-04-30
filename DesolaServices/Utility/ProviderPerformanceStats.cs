namespace DesolaServices.Utility;

public class ProviderPerformanceStats
{
    public int SuccessCount { get; private set; }
    public int FailureCount { get; private set; }
    public TimeSpan TotalResponseTime { get; private set; }

    public double AverageResponseTimeMs => SuccessCount > 0 ? TotalResponseTime.TotalMilliseconds / SuccessCount : 0;

    public void RecordSuccess(TimeSpan responseTime)
    {
        SuccessCount++;
        TotalResponseTime += responseTime;
    }

    public void RecordFailure()
    {
        FailureCount++;
    }
}