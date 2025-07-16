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
            var subscriptions = await _subscriptionService.GetCustomerSubscriptionsAsync(request.StripeCustomerId, cancellationToken);

            var subscriptionDetailsEnumerable = subscriptions.ToList();
            if (!subscriptionDetailsEnumerable.Any())
            {
                _logger.LogWarning("No subscriptions found for customer: {StripeCustomerId}", request.StripeCustomerId);
                return CancelSubscriptionResponse.FailureResult("No subscriptions found for this customer");
            }

            var activeSubscriptions = subscriptionDetailsEnumerable.Where(s =>
                s.Status == "active" ||
                s.Status == "trialing" ||
                s.Status == "past_due").ToList();

            if (!activeSubscriptions.Any())
            {
                _logger.LogInformation("No active subscriptions found for customer: {StripeCustomerId}", request.StripeCustomerId);
                return CancelSubscriptionResponse.SuccessResult(null, "No active subscriptions to cancel", null);
            }

            Customer customer = null;
            if (!string.IsNullOrEmpty(request.StripeCustomerId))
            {
                customer = await _subscribedCustomer.GetCustomerByStripeIdAsync(request.StripeCustomerId, cancellationToken);
            }

            CancelSubscriptionResult lastCancelResult = null;
            var canceledSubscriptions = new List<string>();

            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    var cancelResult = await _subscriptionService.CancelSubscriptionAsync(subscription.Id, !request.CancelAtPeriodEnd, cancellationToken);

                    lastCancelResult = cancelResult;
                    canceledSubscriptions.Add(subscription.Id);

                    _logger.LogInformation("Successfully canceled subscription: {SubscriptionId}, Status: {Status}", subscription.Id, cancelResult.Status);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cancel subscription: {SubscriptionId}", subscription.Id);
                    // Continue with other subscriptions
                }
            }

            if (!canceledSubscriptions.Any())
            {
                _logger.LogError("Failed to cancel any subscriptions for customer: {StripeCustomerId}", request.StripeCustomerId);
                return CancelSubscriptionResponse.FailureResult("Failed to cancel subscriptions");
            }

            if (customer != null && lastCancelResult != null)
            {
                await UpdateCustomerSubscriptionStatus(customer, lastCancelResult, request.CancelAtPeriodEnd, cancellationToken);
            }

            _logger.LogInformation("Successfully processed subscription cancellation for customer: {StripeCustomerId}, Canceled subscriptions: {Count}",
                request.StripeCustomerId, canceledSubscriptions.Count);

            return CancelSubscriptionResponse.SuccessResult(canceledSubscriptions.FirstOrDefault(), lastCancelResult?.Status ?? "canceled", customer?.SubscriptionExpiresAt ?? DateTime.UtcNow);
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
                customer.SubscriptionExpiresAt = cancelResult.CanceledAt ?? DateTime.UtcNow.AddDays(30);
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
