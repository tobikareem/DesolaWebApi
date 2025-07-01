namespace DesolaServices.DataTransferObjects.Responses;

public class CancelSubscriptionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CancelledAt { get; set; }
    public List<string> Errors { get; set; } = new();

    public static CancelSubscriptionResponse SuccessResult(string subscriptionId, string status, DateTime? cancelledAt)
    {
        return new CancelSubscriptionResponse
        {
            Success = true,
            Message = "Subscription cancelled successfully",
            SubscriptionId = subscriptionId,
            Status = status,
            CancelledAt = cancelledAt
        };
    }

    public static CancelSubscriptionResponse FailureResult(string message, List<string> errors = null)
    {
        return new CancelSubscriptionResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}