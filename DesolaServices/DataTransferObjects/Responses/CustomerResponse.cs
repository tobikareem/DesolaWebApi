namespace DesolaServices.DataTransferObjects.Responses;

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string StripeCustomerId { get; set; }
    public bool HasActiveSubscription { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
    public string PreferredCurrency { get; set; }
    public string DefaultOriginAirport { get; set; }
    public DateTimeOffset? LastActiveAt { get; set; }
}