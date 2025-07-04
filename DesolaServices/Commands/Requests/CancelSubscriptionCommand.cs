using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Requests;

public class CancelSubscriptionCommand : IRequest<CancelSubscriptionResponse>
{
    public string StripeCustomerId { get; set; }
    public string CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public bool CancelAtPeriodEnd { get; set; } = true;
    public string CancellationReason { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

    public CancelSubscriptionCommand() { }

    public CancelSubscriptionCommand(string subscriptionId, bool cancelAtPeriodEnd = true, string cancellationReason = null)
    {
        StripeCustomerId = subscriptionId;
        CancelAtPeriodEnd = cancelAtPeriodEnd;
        CancellationReason = cancellationReason;
    }
}