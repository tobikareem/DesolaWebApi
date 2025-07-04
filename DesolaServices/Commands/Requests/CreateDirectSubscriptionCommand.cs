using CaptainPayment.Core.Models;
using MediatR;

namespace DesolaServices.Commands.Requests;

public class CreateDirectSubscriptionCommand : IRequest<CreateSubscriptionResult>
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PaymentMethodId { get; set; } = string.Empty;
    public string PriceId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public int TrialPeriodDays { get; set; } = 7;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}