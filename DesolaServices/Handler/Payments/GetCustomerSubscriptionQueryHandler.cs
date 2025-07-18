using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using DesolaServices.Commands.Queries.Payment;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Stripe;
using Customer = DesolaDomain.Entities.User.Customer;

namespace DesolaServices.Handler.Payments;

public class GetCustomerSubscriptionQueryHandler : IRequestHandler<GetCustomerSubscriptionQuery, CustomerSubscriptionResponse>
{
    private readonly ICustomerManagementService _customerManagementService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<GetCustomerSubscriptionQueryHandler> _logger;

    public GetCustomerSubscriptionQueryHandler(
        ICustomerManagementService subscribedCustomer,
        ISubscriptionService subscriptionService,
        ILogger<GetCustomerSubscriptionQueryHandler> logger)
    {
        _customerManagementService = subscribedCustomer;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    public async Task<CustomerSubscriptionResponse> Handle(GetCustomerSubscriptionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing customer subscription request for: {Email} {StripeCustomerId} {CustomerId}",
            request.Request.Email, request.Request.StripeCustomerId, request.Request.CustomerId);

        try
        {
            // Step 1: Find the customer
            Customer customer = null;

            if (!string.IsNullOrEmpty(request.Request.Email))
            {
                customer = await _customerManagementService.GetCustomerAsync(request.Request.Email, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(request.Request.StripeCustomerId))
            {
                customer = await _customerManagementService.GetCustomerByStripeIdAsync(request.Request.StripeCustomerId, cancellationToken);
            }

            if (customer == null)
            {
                _logger.LogWarning("Customer not found for request: {Email} {StripeCustomerId}",
                    request.Request.Email, request.Request.StripeCustomerId);
                return null;
            }

            // Step 2: Get subscription details from Stripe
            var subscriptions = new List<SubscriptionDetails>();
            var hasActiveSubscription = false;
            var currentPlan = "None";
            var currentAmount = 0m;
            var currency = "USD";
            var subscriptionExpiresAt = customer.SubscriptionExpiresAt;
            var trialEnd = (DateTime?)null;
            var isTrialing = false;
            var nextBillingDate = (DateTime?)null;
            var status = "inactive";

            if (!string.IsNullOrEmpty(customer.StripeCustomerId))
            {
                try
                {
                    var subscriptionDetails = await _subscriptionService.GetCustomerSubscriptionsAsync(customer.StripeCustomerId, cancellationToken);

                    subscriptions = subscriptionDetails.ToList();
                    foreach (var sub in subscriptions.Where(sub => sub.Status == "active" || sub.Status == "trialing"))
                    {
                        hasActiveSubscription = true;
                        currentPlan = DeterminePlanName(sub.Interval);
                        currentAmount = sub.Amount;
                        currency = sub.Currency;
                        status = sub.Status;
                        subscriptionExpiresAt = sub.SubscriptionEndDate;
                        if (sub.Status == "trialing")
                        {
                            isTrialing = true;
                            trialEnd = sub.TrialEnd;
                        }
                        
                        if (sub.TrialEnd.HasValue && sub.TrialEnd > DateTime.UtcNow)
                        {
                            nextBillingDate = sub.TrialEnd;
                        }
                        else
                        {
                            nextBillingDate = CalculateNextBillingDate(sub.CreatedAt, sub.Interval);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching Stripe subscriptions for customer: {StripeCustomerId}", customer.StripeCustomerId);
                }
            }

            // Step 3: Build response
            var response = new CustomerSubscriptionResponse
            {
                Customer = new Customer
                {
                    Email = customer.Email,
                    FullName = customer.FullName,
                    Phone = customer.Phone,
                    StripeCustomerId = customer.StripeCustomerId,
                    CreatedAt = customer.CreatedAt,
                    LastActiveAt = customer.LastActiveAt,
                    PreferredCurrency = customer.PreferredCurrency ?? "USD",
                    DefaultOriginAirport = customer.DefaultOriginAirport
                },
                Subscriptions = subscriptions,
                HasActiveSubscription = hasActiveSubscription || customer.HasActiveSubscription,
                SubscriptionExpiresAt = subscriptionExpiresAt,
                CurrentPlan = currentPlan,
                CurrentAmount = currentAmount,
                Currency = currency,
                TrialEnd = trialEnd,
                IsTrialing = isTrialing,
                NextBillingDate = nextBillingDate,
                Status = status
            };

            _logger.LogInformation("Successfully retrieved subscription information for customer: {Email}", customer.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer subscription request");
            throw;
        }
    }

    private static string DeterminePlanName(string interval)
    {
        return interval?.ToLower() switch
        {
            "month" => "Monthly",
            "year" => "Yearly",
            _ => $"{interval} Plan"
        };
    }

    private static DateTime? CalculateNextBillingDate(DateTime createdAt, string interval)
    {
        return interval?.ToLower() switch
        {
            "month" => createdAt.AddMonths(1),
            "year" => createdAt.AddYears(1),
            _ => createdAt.AddMonths(1)
        };
    }
}