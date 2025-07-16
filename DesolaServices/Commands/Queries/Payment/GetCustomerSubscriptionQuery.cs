using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Queries.Payment;

public class GetCustomerSubscriptionQuery : IRequest<CustomerSubscriptionResponse>
{
    public GetCustomerSubscriptionRequest Request { get; }

    public GetCustomerSubscriptionQuery(GetCustomerSubscriptionRequest request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }
}