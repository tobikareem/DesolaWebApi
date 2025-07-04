using DesolaDomain.Entities.User;
using DesolaServices.Commands.Queries.Payment;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Payments;

public class GetCustomerByEmailQueryHandler: IRequestHandler<GetCustomerByEmailQuery, Customer>
{
    private readonly ILogger<GetCustomerByEmailQueryHandler> _logger;
    private readonly ICustomerManagementService _customerManagementService;

    public GetCustomerByEmailQueryHandler(ILogger<GetCustomerByEmailQueryHandler> logger, ICustomerManagementService customerManagementService)
    {
        _logger = logger;
        _customerManagementService = customerManagementService;
    }

    public async Task<Customer> Handle(GetCustomerByEmailQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerManagementService.GetCustomerAsync(request.Email, cancellationToken);
        if (customer != null) return customer;
        _logger.LogWarning($"No customer found with email: {request.Email}");
        return null;
    }

}