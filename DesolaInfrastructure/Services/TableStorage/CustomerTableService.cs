using Azure;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaInfrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.Services.TableStorage;

public class CustomerTableService : BaseTableStorage<Customer>, ICustomerTableService
{
    public CustomerTableService 
        (ITableStorageRepository<Customer> storageRepository, 
            ICacheService cacheService, 
            ILogger<CustomerTableService> logger, 
            IOptions<AppSettings> configuration, TimeSpan? 
                cacheExpiration = null) 
        : base(storageRepository, cacheService, logger, configuration.Value.Database.CustomerTableName, cacheExpiration)
    {
    }

    protected override string GetPartitionKey(Customer entity)
    {
        var emailDomain = entity.Email?.Split('@').LastOrDefault()?.ToLowerInvariant() ?? "unknown";
        return $"domain_{emailDomain}";
    }

    protected override string GetRowKey(Customer entity)
    {
        return entity.Email?.ToLowerInvariant() ?? entity.Id.ToString();
    }

    protected override ETag GetETag(Customer entity) => entity.ETag;

    protected override void SetETag(Customer entity, ETag etag)
    {
        entity.ETag = etag;
    }

    public async Task<Customer> GetByEmailAsync(string email)
    {
        var partitionKey = GetPartitionKeyFromEmail(email);
        var rowKey = email.ToLowerInvariant();
        return await GetTableEntityAsync(partitionKey, rowKey);
    }

    public async Task<Customer> GetByStripeCustomerIdAsync(string stripeCustomerId)
    {
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            throw new ArgumentException("Stripe customer ID cannot be null or empty", nameof(stripeCustomerId));

        var cacheKey = $"stripe_customer_{stripeCustomerId}";

        var cachedCustomer = CacheService.GetItem<Customer>(cacheKey);

        if (cachedCustomer != null)
        {
            Logger.LogInformation("Cache hit for Stripe customer ID: {StripeCustomerId}", stripeCustomerId);
            return cachedCustomer;
        }

        var query = $"StripeCustomerId eq '{stripeCustomerId}'";
        var (customers, _) = await GetTableEntitiesByQueryAsync(query, 1, null);

        var customer = customers.FirstOrDefault();
        if (customer != null)
        {
            CacheService.Add(cacheKey, customer, TimeSpan.FromHours(1));
            Logger.LogInformation("Found customer by Stripe ID: {StripeCustomerId}", stripeCustomerId);
        }
        else
        {
            Logger.LogWarning("Customer not found for Stripe ID: {StripeCustomerId}", stripeCustomerId);
        }

        return customer;
    }

    public async Task<IEnumerable<Customer>> GetActiveSubscribersAsync()
    {
        var query = $"{nameof(Customer.HasActiveSubscription)} eq true";
        var allCustomers = new List<Customer>();
        string continuationToken = null;

        do
        {
            var (customers, nextToken) = await GetTableEntitiesByQueryAsync(query, 100, continuationToken);
            allCustomers.AddRange(customers);
            continuationToken = nextToken;
        }
        while (!string.IsNullOrEmpty(continuationToken));

        Logger.LogInformation("Retrieved {Count} active subscribers", allCustomers.Count);

        return allCustomers;
    }

    public async Task<IEnumerable<Customer>> GetCustomersByDomainAsync(string domain, int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain cannot be null or empty", nameof(domain));

        try
        {
            var partitionKey = $"domain_{domain.ToLowerInvariant()}";
            var query = $"PartitionKey eq '{partitionKey}'";

            var (customers, _) = await GetTableEntitiesByQueryAsync(query, limit, null);

            Logger.LogInformation("Retrieved {Count} customers for domain {Domain}", customers.Count, domain);
            return customers;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get customers for domain {Domain}", domain);
            throw;
        }
    }

    public async Task<bool> UpdateSubscriptionStatusAsync(string stripeCustomerId, bool hasActiveSubscription, DateTime? subscriptionExpiresAt = null, string subscriptionId = null)
    {
        try
        {
            var customer = await GetByStripeCustomerIdAsync(stripeCustomerId);
            if (customer == null)
            {
                Logger.LogWarning("Customer not found for Stripe ID: {StripeCustomerId}", stripeCustomerId);
                return false;
            }

            customer.HasActiveSubscription = hasActiveSubscription;
            customer.SubscriptionExpiresAt = subscriptionExpiresAt;
            customer.CurrentSubscriptionId = subscriptionId;
            customer.LastActiveAt = DateTime.UtcNow;

            await UpdateTableEntityAsync(customer);

            Logger.LogInformation("Updated subscription status for customer {Email}: Active={IsActive}, Expires={ExpiresAt}",
                customer.Email, hasActiveSubscription, subscriptionExpiresAt);

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating subscription status for Stripe customer {StripeCustomerId}", stripeCustomerId);
            return false;
        }
    }

    private string GetPartitionKeyFromEmail(string email)
    {
        var emailDomain = email?.Split('@').LastOrDefault()?.ToLowerInvariant() ?? "unknown";
        return $"domain_{emailDomain}";
    }
}