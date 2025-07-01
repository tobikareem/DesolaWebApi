namespace DesolaServices.DataTransferObjects.Responses;

public class CreatePaymentIntentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static CreatePaymentIntentResponse SuccessResult(string paymentIntentId, string clientSecret, string status)
    {
        return new CreatePaymentIntentResponse
        {
            Success = true,
            Message = "Payment intent created successfully",
            PaymentIntentId = paymentIntentId,
            ClientSecret = clientSecret,
            Status = status
        };
    }

    public static CreatePaymentIntentResponse FailureResult(string message, List<string> errors = null)
    {
        return new CreatePaymentIntentResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}