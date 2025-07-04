namespace DesolaServices.DataTransferObjects.Responses;

public class ConfirmPaymentIntentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static ConfirmPaymentIntentResponse SuccessResult(string paymentIntentId, string status)
    {
        return new ConfirmPaymentIntentResponse
        {
            Success = true,
            Message = "Payment intent confirmed successfully",
            PaymentIntentId = paymentIntentId,
            Status = status
        };
    }

    public static ConfirmPaymentIntentResponse FailureResult(string message, List<string> errors = null)
    {
        return new ConfirmPaymentIntentResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}