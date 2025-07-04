using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using DesolaDomain.Entities.User;
using DesolaServices.Commands.Requests;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Payments;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, CancelSubscriptionResponse>
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICustomerManagementService _subscribedCustomer;
    private readonly ILogger<CancelSubscriptionCommandHandler> _logger;

    public CancelSubscriptionCommandHandler(ISubscriptionService subscriptionService, ICustomerManagementService customerService, ILogger<CancelSubscriptionCommandHandler> logger)
    {
        _subscriptionService = subscriptionService;
        _subscribedCustomer = customerService;
        _logger = logger;
    }

    public async Task<CancelSubscriptionResponse> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing subscription cancellation for subscription: {SubscriptionId}", request.StripeCustomerId);

        try
        {
            var subscription = await _subscriptionService.GetSubscriptionAsync(request.StripeCustomerId, cancellationToken);
            
            if (subscription.Status == "canceled" || subscription.Status == "incomplete_expired")
            {
                _logger.LogInformation("Subscription already canceled: {SubscriptionId}", request.StripeCustomerId);
                return new CancelSubscriptionResponse
                {
                    SubscriptionId = request.StripeCustomerId,
                    Status = subscription.Status,
                    Success = true,
                    IsActive = false,
                    Message = "Subscription was already canceled"
                };
            }

            // Step 3: Get customer information for our records
            Customer customer = null;
            if (!string.IsNullOrEmpty(request.StripeCustomerId))
            {
                customer = await _subscribedCustomer.GetCustomerByStripeIdAsync(subscription.CustomerId, cancellationToken);
            }

            // Step 4: Cancel subscription via Stripe
            var cancelResult = await _subscriptionService.CancelSubscriptionAsync(request.StripeCustomerId, request.CancelAtPeriodEnd, cancellationToken);

            // Step 5: Update customer subscription status in our database
            if (customer != null)
            {
                await UpdateCustomerSubscriptionStatus(customer, cancelResult, request.CancelAtPeriodEnd, cancellationToken);
            }
            
            

            _logger.LogInformation("Successfully processed subscription cancellation: {SubscriptionId}, Status: {Status}",
                request.StripeCustomerId, cancelResult.Status);

            return CancelSubscriptionResponse.SuccessResult(customer?.CurrentSubscriptionId, cancelResult.Status, customer?.SubscriptionExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling subscription: {SubscriptionId}", request.StripeCustomerId);
            return CancelSubscriptionResponse.FailureResult($"Error canceling subscription: {request.StripeCustomerId}");
        }
    }

    private async Task UpdateCustomerSubscriptionStatus(Customer customer, CancelSubscriptionResult cancelResult, bool cancelAtPeriodEnd, CancellationToken cancellationToken)
    {
        try
        {
            if (cancelAtPeriodEnd)
            {
                // Keep subscription active until period end
                customer.HasActiveSubscription = true;
                customer.SubscriptionExpiresAt = cancelResult.CanceledAt;
            }
            else
            {
                // Immediate cancellation
                customer.HasActiveSubscription = false;
                customer.SubscriptionExpiresAt = DateTime.UtcNow;
            }

            customer.CurrentSubscriptionId = null;
            customer.LastActiveAt = DateTime.UtcNow;

            await _subscribedCustomer.UpdateSubscriptionStatusAsync(customer.StripeCustomerId, customer.HasActiveSubscription, customer.SubscriptionExpiresAt, customer.CurrentSubscriptionId, cancellationToken);
            _logger.LogInformation("Updated customer subscription status: {Email}", customer.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update customer subscription status: {CustomerId}", customer.Id);
        }
    }
}
