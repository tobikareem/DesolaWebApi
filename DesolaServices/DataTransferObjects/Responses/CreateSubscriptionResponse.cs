namespace DesolaServices.DataTransferObjects.Responses;

public class CreateSubscriptionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static CreateSubscriptionResponse SuccessResult(string subscriptionId, string status, string clientSecret = null)
    {
        return new CreateSubscriptionResponse
        {
            Success = true,
            Message = "Subscription created successfully",
            SubscriptionId = subscriptionId,
            Status = status,
            ClientSecret = clientSecret
        };
    }

    public static CreateSubscriptionResponse FailureResult(string message, List<string> errors = null)
    {
        return new CreateSubscriptionResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}