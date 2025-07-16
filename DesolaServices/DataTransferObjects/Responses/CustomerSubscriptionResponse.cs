using CaptainPayment.Core.Models;
using DesolaDomain.Entities.User;

namespace DesolaServices.DataTransferObjects.Responses;

public class CustomerSubscriptionResponse
{
    public Customer Customer { get; set; }
    public List<SubscriptionDetails> Subscriptions { get; set; } = new();
    public bool HasActiveSubscription { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
    public string CurrentPlan { get; set; }
    public decimal CurrentAmount { get; set; }
    public string Currency { get; set; }
    public DateTime? TrialEnd { get; set; }
    public bool IsTrialing { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public string Status { get; set; }
}