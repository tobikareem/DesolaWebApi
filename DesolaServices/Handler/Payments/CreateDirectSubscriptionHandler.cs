using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using DesolaServices.Commands.Requests;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Payments;

public class CreateDirectSubscriptionHandler : IRequestHandler<CreateDirectSubscriptionCommand, CreateSubscriptionResult>
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICustomerManagementService _customerService;
    private readonly ILogger<CreateDirectSubscriptionHandler> _logger;

    public CreateDirectSubscriptionHandler(ISubscriptionService subscriptionService, ICustomerManagementService customerService, ILogger<CreateDirectSubscriptionHandler> logger)
    {
        _subscriptionService = subscriptionService;
        _customerService = customerService;
        _logger = logger;
    }
    public async Task<CreateSubscriptionResult> Handle(CreateDirectSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating direct subscription for: {Email}", request.Email);

        var customer = await _customerService.GetCustomerAsync(request.Email, cancellationToken);

        if (customer == null)
        {
            var signUpRequest = new CustomerSignupRequest
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone
            };
            await _customerService.CreateCustomerAsync(signUpRequest, cancellationToken);

            customer = await _customerService.GetCustomerAsync(request.Email, cancellationToken);
        }

        if(request.CustomerId != null && request.CustomerId != customer.StripeCustomerId)
        {
            _logger.LogWarning("Provided CustomerId {CustomerId} does not match existing customer Stripe ID {StripeCustomerId}", request.CustomerId, customer.StripeCustomerId);
            throw new InvalidOperationException($"Provided CustomerId {request.CustomerId} does not match existing customer Stripe ID {customer.StripeCustomerId}");
        }

        var subscriptionRequest = new CreateSubscriptionRequest
        {
            CustomerId = customer.StripeCustomerId,
            Email = request.Email,
            FullName = request.FullName,
            PriceId = request.PriceId,
            PaymentMethodId = request.PaymentMethodId,
            TrialPeriodDays = request.TrialPeriodDays,
            Metadata = request.Metadata
        };

        var result = await _subscriptionService.CreateSubscriptionAsync(subscriptionRequest, cancellationToken);
        
        await _customerService.UpdateSubscriptionStatusAsync(
            customer.StripeCustomerId,
            result.Status is "active" or "trialing",
            result.TrialEnd.GetValueOrDefault(),
            result.SubscriptionId, cancellationToken);

        _logger.LogInformation("Direct subscription created: {SubscriptionId}", result.SubscriptionId);
        return result;
    }
}