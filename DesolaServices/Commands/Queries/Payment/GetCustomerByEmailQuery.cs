
using DesolaDomain.Entities.User;
using MediatR;

namespace DesolaServices.Commands.Queries.Payment;

public class GetCustomerByEmailQuery: IRequest<Customer>
{
    public string Email { get; }

    public GetCustomerByEmailQuery(string email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email), "Email cannot be null");
    }
}