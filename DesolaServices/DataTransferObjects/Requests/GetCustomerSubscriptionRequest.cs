namespace DesolaServices.DataTransferObjects.Requests;

public class GetCustomerSubscriptionRequest
{
    public string Email { get; set; }
    public string StripeCustomerId { get; set; }
    public string CustomerId { get; set; }
}