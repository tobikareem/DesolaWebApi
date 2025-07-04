using DesolaDomain.Entities.User;

namespace DesolaDomain.Interfaces;

/// <summary>
/// Customer-specific repository interface extending the base table operations
/// </summary>
public interface ICustomerTableService : ITableBase<Customer>
{
    Task<Customer> GetByEmailAsync(string email);
    Task<Customer> GetByStripeCustomerIdAsync(string stripeCustomerId);
    Task<IEnumerable<Customer>> GetActiveSubscribersAsync();
    Task<IEnumerable<Customer>> GetCustomersByDomainAsync(string domain, int limit = 100);
    Task<bool> UpdateSubscriptionStatusAsync(string stripeCustomerId, bool hasActiveSubscription, DateTime? subscriptionExpiresAt = null, string subscriptionId = null);
}