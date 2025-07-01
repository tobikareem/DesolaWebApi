using DesolaDomain.Entities.User;
using DesolaServices.Commands.Queries.Payment;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Payments;

public class GetCustomerByEmailQueryHandler: IRequest<GetCustomerByEmailQuery>
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
        _logger.LogInformation($"Fetching customer by email: {request.Email}");
        var customer = await _customerManagementService.GetByEmailAsync(request.Email);
        if (customer == null)
        {
            _logger.LogWarning($"No customer found with email: {request.Email}");
            return new Customer();
        }
        _logger.LogInformation($"Customer found: {customer.Email}");
        return customer;
    }

}