using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using DesolaDomain.Entities.Payment;
using DesolaServices.Commands.Requests;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Payments;

public class PaymentSetupIntentHandler : IRequestHandler<CreateSetupIntentCommand, SetupIntentResult>
{
    private readonly ISetupIntentService _setupIntentService;
    private readonly ILogger<PaymentSetupIntentHandler> _logger;
    private readonly IPaymentIntentResultService _paymentIntentResultService;
    private readonly ICustomerManagementService _customerService;
    public PaymentSetupIntentHandler(ISetupIntentService setupIntentService, ILogger<PaymentSetupIntentHandler> logger, IPaymentIntentResultService paymentIntentResultService, ICustomerManagementService customerService)
    {
        _setupIntentService = setupIntentService;
        _logger = logger;
        _paymentIntentResultService = paymentIntentResultService;
        _customerService = customerService;
    }

    public async Task<SetupIntentResult> Handle(CreateSetupIntentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating setup intent for request: {CustomerId}", request.CreateSetupIntentRequest.CustomerId);
        if (string.IsNullOrWhiteSpace(request.CreateSetupIntentRequest.CustomerId))
        {
            _logger.LogError("Setup intent request is null");
            throw new ArgumentNullException(nameof(request.CreateSetupIntentRequest.CustomerId), "Setup intent request for CustomerId cannot be null");
        }

        var customer = await _customerService.GetCustomerByStripeIdAsync(request.CreateSetupIntentRequest.CustomerId, cancellationToken);

        if (customer == null)
        {
            _logger.LogError("Customer not found for Stripe ID: {StripeCustomerId}", request.CreateSetupIntentRequest.CustomerId);
            throw new InvalidOperationException($"Customer with Stripe ID {request.CreateSetupIntentRequest.CustomerId} does not exist");
        }

        var result = await _setupIntentService.CreateSetupIntentAsync(request.CreateSetupIntentRequest, cancellationToken);

        if (result == null)
        {
            _logger.LogError("Failed to create setup intent for request: {@Request}", request.CreateSetupIntentRequest.CustomerId);
            throw new InvalidOperationException("Failed to create setup intent");
        }

        await _paymentIntentResultService.SavePaymentIntentAsync(result, request.UserId);
        _logger.LogInformation("Successfully created setup intent with ID: {SetupIntentId}", result.Id);
        return result;
    }
}