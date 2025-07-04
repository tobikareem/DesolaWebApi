using DesolaDomain.Entities.Payment;
using MediatR;

namespace DesolaServices.Commands.Queries.Payment;

public class GetSetupIntentQuery:IRequest<IEnumerable<PaymentIntentResult>>
{
    public string CustomerId { get; }

    public GetSetupIntentQuery(string customerId)
    {
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId), "CustomerId cannot be null");
    }
}