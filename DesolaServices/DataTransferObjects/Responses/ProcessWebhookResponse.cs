namespace DesolaServices.DataTransferObjects.Responses;

public class ProcessWebhookResponse
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}