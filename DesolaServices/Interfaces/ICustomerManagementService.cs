using DesolaDomain.Entities.User;
using DesolaServices.DataTransferObjects.Requests;

namespace DesolaServices.Interfaces;

public interface ICustomerManagementService
{
    /// <summary>
    /// Gets customer by email with proper synchronization between local and Stripe data
    /// </summary>
    Task<Customer> GetCustomerAsync(string email, CancellationToken cancellationToken = default);


    /// <summary>
    /// Gets customer by Stripe Customer ID
    /// </summary>
    Task<Customer> GetCustomerByStripeIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default);



    /// <summary>
    /// Creates a new customer with Stripe integration
    /// </summary>
    Task<(bool Success, Customer Customer, string StripeCustomerId, string ErrorMessage)> CreateCustomerAsync(CustomerSignupRequest request, CancellationToken cancellationToken = default);


    /// <summary>
    /// Updates customer subscription status
    /// </summary>
    Task<bool> UpdateSubscriptionStatusAsync(string stripeCustomerId, bool hasActiveSubscription, DateTime? subscriptionExpiresAt, string subscriptionId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates customer profile information and syncs with Stripe
    /// </summary>
    Task<bool> UpdateCustomerProfileAsync(string email, string fullName = null, string phone = null, string preferredCurrency = null, string defaultOriginAirport = null, Dictionary<string, string> additionalMetadata = null, CancellationToken cancellationToken = default);
}