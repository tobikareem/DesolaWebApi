using DesolaDomain.Entities.User;
using DesolaDomain.Model;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Interfaces;

public interface ICustomerManagementService
{
    /// <summary>
    /// Gets a customer by their email address.
    /// </summary>
    /// <param name="email">The customer's email address.</param>
    /// <returns>The customer if found, null otherwise.</returns>
    Task<Customer> GetByEmailAsync(string email);
    /// <summary>
    /// Gets a customer by their Stripe customer ID.
    /// Note: This is less efficient as it requires scanning. Consider caching or indexing.
    /// </summary>
    /// <param name="stripeCustomerId">The Stripe customer ID.</param>
    /// <returns>The customer if found, null otherwise.</returns>
    Task<Customer> GetByStripeCustomerIdAsync(string stripeCustomerId);

    /// <summary>
    /// Creates a new customer record.
    /// </summary>
    /// <param name="customer">The customer to create.</param>
    /// <param name="tokenSource"></param>
    /// <returns>The created customer with updated properties.</returns>
    Task<CustomerCreationResult> CreateCustomerAsync(CustomerSignupRequest customer, CancellationToken tokenSource);

    /// <summary>
    /// Updates an existing customer record.
    /// </summary>
    /// <param name="customer">The customer to update.</param>
    /// <returns>The updated customer.</returns>
    Task<Customer> UpdateCustomerAsync(Customer customer);

    /// <summary>
    /// Gets all customers with active subscriptions.
    /// Note: This is expensive for large datasets. Consider using a separate index or query optimization.
    /// </summary>
    /// <returns>List of customers with active subscriptions.</returns>
    Task<IEnumerable<Customer>> GetActiveSubscribersAsync();

    /// <summary>
    /// Updates the subscription status for a customer.
    /// </summary>
    /// <param name="stripeCustomerId">The Stripe customer ID.</param>
    /// <param name="hasActiveSubscription">Whether the subscription is active.</param>
    /// <param name="subscriptionExpiresAt">When the subscription expires (optional).</param>
    /// <param name="subscriptionId">The subscription ID (optional).</param>
    /// <returns>True if updated successfully, false otherwise.</returns>
    Task<bool> UpdateSubscriptionStatusAsync(string stripeCustomerId, bool hasActiveSubscription, DateTime? subscriptionExpiresAt = null, string subscriptionId = null);

    /// <summary>
    /// Gets customers by domain (email domain).
    /// </summary>
    /// <param name="domain">The email domain (e.g., "gmail.com").</param>
    /// <param name="limit">Maximum number of customers to return.</param>
    /// <returns>List of customers from the specified domain.</returns>
    Task<IEnumerable<Customer>> GetCustomersByDomainAsync(string domain, int limit = 100);
}