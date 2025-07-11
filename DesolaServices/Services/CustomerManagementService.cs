using System.Text.Json;
using AutoMapper;
using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.Interfaces;
using DesolaServices.Utility;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Services;

public class CustomerManagementService : ICustomerManagementService
{
    private readonly ICustomerTableService _customerTableService;
    private readonly ILogger<CustomerManagementService> _logger;
    private readonly ICustomerService _stripeCustomerService;
    private readonly IMapper _mapper;

    public CustomerManagementService(ICustomerTableService table, ILogger<CustomerManagementService> logger, ICustomerService customerService, IMapper mapper)
    {
        _customerTableService = table;
        _logger=logger;
        _stripeCustomerService = customerService;
        _mapper = mapper;
    }


    /// <summary>
    /// Gets customer by email with proper synchronization between local and Stripe data
    /// </summary>
    public async Task<Customer> GetCustomerAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        try
        {
            // Get local customer first
            var localCustomer = await GetLocalCustomerAsync(email);

            if (localCustomer == null)
            {
                _logger.LogInformation("Customer not found locally, checking Stripe: {Email}", email);
                return await GetCustomerFromStripeAndCache(email, cancellationToken);
            }

            if (IsCustomerDataFresh(localCustomer))
            {
                _logger.LogDebug("Using fresh local customer data: {Email}", email);
                return localCustomer;
            }

            _logger.LogInformation("Local customer data is stale, refreshing from Stripe: {Email}", email);
            return await RefreshCustomerFromStripe(localCustomer, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Gets customer by Stripe Customer ID
    /// </summary>
    public async Task<Customer> GetCustomerByStripeIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            throw new ArgumentException("Stripe customer ID cannot be null or empty", nameof(stripeCustomerId));

        try
        {
            _logger.LogInformation("Getting customer by Stripe ID: {StripeCustomerId}", stripeCustomerId);

            // Get from Stripe first (source of truth for Stripe ID)
            var stripeCustomer = await _stripeCustomerService.GetCustomerAsync(stripeCustomerId, cancellationToken);

            // Check if we have this customer locally
            var localCustomer = await GetLocalCustomerAsync(stripeCustomer.Email);

            return await HandleCustomerSyncScenario(localCustomer, stripeCustomer, stripeCustomer.Email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by Stripe ID: {StripeCustomerId}", stripeCustomerId);
            throw;
        }
    }


    /// <summary>
    /// Creates a new customer with Stripe integration
    /// </summary>
    public async Task<(bool Success, Customer Customer, string StripeCustomerId, string ErrorMessage)> CreateCustomerAsync(CustomerSignupRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        try
        {
            _logger.LogInformation("Creating customer: {Email}", request.Email);

            var existingCustomer = await GetLocalCustomerAsync(request.Email);
            if (existingCustomer != null)
            {
                _logger.LogWarning("Customer already exists: {Email}", request.Email);
                return (false, null, null, "Customer already exists");
            }

            // Create customer entity
            var customer = _mapper.Map<Customer>(request);
            customer.SetTableStorageKeys();

            // Validate customer
            if (!customer.IsValid(out var validationErrors))
            {
                var errorMessage = string.Join(", ", validationErrors);
                _logger.LogError("Customer validation failed: {Errors}", errorMessage);
                return (false, null, null, errorMessage);
            }

            // Create in Stripe first
            var stripeRequest = _mapper.Map<CreateCustomerRequest>(request);
            var stripeCustomer = await _stripeCustomerService.CreateCustomerAsync(stripeRequest, cancellationToken);

            // Update customer with Stripe ID
            customer.StripeCustomerId = stripeCustomer.Id;

            // Store locally
            await SafeInsertCustomerAsync(customer);

            _logger.LogInformation("Customer created successfully: {Email}, StripeId: {StripeId}",
                customer.Email, customer.StripeCustomerId);

            return (true, customer, stripeCustomer.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {Email}", request.Email);
            return (false, null, null, "An unexpected error occurred");
        }
    }

    /// <summary>
    /// Updates customer subscription status
    /// </summary>
    public async Task<bool> UpdateSubscriptionStatusAsync(string stripeCustomerId, bool hasActiveSubscription, DateTime? subscriptionExpiresAt, string subscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating subscription status for Stripe customer: {StripeCustomerId}", stripeCustomerId);
            var customer = await GetCustomerByStripeIdAsync(stripeCustomerId, cancellationToken);
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for subscription update: {StripeCustomerId}", stripeCustomerId);
                return false;
            }

            customer.HasActiveSubscription = hasActiveSubscription;
            customer.SubscriptionExpiresAt = subscriptionExpiresAt;
            customer.CurrentSubscriptionId = subscriptionId ?? string.Empty;
            customer.LastActiveAt = DateTime.UtcNow;

            // Update metadata
            customer.SetMetadata("subscription_status", hasActiveSubscription ? "active" : "inactive");
            customer.SetMetadata("last_subscription_update", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            await SafeUpdateCustomerTableStorageAsync(customer);

            // Sync with Stripe using the new mapper
            var updateRequest = _mapper.Map<UpdateCustomerRequest>(customer);
            await _stripeCustomerService.UpdateCustomerAsync(customer.StripeCustomerId, updateRequest, cancellationToken);

            _logger.LogInformation("Updated subscription status for customer {Email}: Active={IsActive}",
                customer.Email, hasActiveSubscription);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription status for Stripe customer {StripeCustomerId}", stripeCustomerId);
            return false;
        }
    }

    public async Task<bool> UpdateCustomerProfileAsync(Customer existingCustomer, List<string> updatedFields, CancellationToken cancellationToken = default)
    {
        if (existingCustomer == null)
            throw new ArgumentNullException(nameof(existingCustomer));

        if (updatedFields == null || !updatedFields.Any())
        {
            _logger.LogInformation("No fields to update for customer: {Email}", existingCustomer.Email);
            return true;
        }

        try
        {
            _logger.LogInformation("Updating customer profile: {Email}, Fields: {Fields}",
                existingCustomer.Email, string.Join(", ", updatedFields));
            
            existingCustomer.LastActiveAt = DateTime.UtcNow;
            existingCustomer.SetMetadata("last_profile_update", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            existingCustomer.SetMetadata("updated_fields", string.Join(",", updatedFields));
            
            var stripeRelevantFields = updatedFields.Where(field =>
                field.Equals("FullName", StringComparison.OrdinalIgnoreCase) ||
                field.Equals("Phone", StringComparison.OrdinalIgnoreCase) ||
                field.Equals("Metadata", StringComparison.OrdinalIgnoreCase)).ToArray();
            
            if (stripeRelevantFields.Any() && !string.IsNullOrWhiteSpace(existingCustomer.StripeCustomerId))
            {
                try
                {
                    _logger.LogInformation("Syncing profile changes with Stripe for customer {Email}: {Fields}",
                        existingCustomer.Email, string.Join(", ", stripeRelevantFields));
                    
                    var updateRequest = existingCustomer.ToUpdateRequest(stripeRelevantFields);
                    updateRequest.Email = existingCustomer.Email;
                    updateRequest.Description = existingCustomer.MetadataJson;
                    updateRequest.Metadata = existingCustomer.Metadata;

                    await _stripeCustomerService.UpdateCustomerAsync(
                        existingCustomer.StripeCustomerId,
                        updateRequest,
                        cancellationToken);

                    _logger.LogInformation("Successfully synced profile changes with Stripe for customer {Email}",
                        existingCustomer.Email);
                    
                    existingCustomer.RemoveMetadata("stripe_profile_sync_failed");
                    existingCustomer.SetMetadata("last_stripe_sync", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                    
                    await SafeUpdateCustomerTableStorageAsync(existingCustomer);
                }
                catch (Exception stripeEx)
                {
                    _logger.LogError(stripeEx,
                        "Failed to sync profile changes with Stripe for customer {Email}. Local update aborted.",
                        existingCustomer.Email);
                    
                    existingCustomer.SetMetadata("stripe_profile_sync_failed", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                    existingCustomer.SetMetadata("stripe_sync_error", stripeEx.Message);

                    throw new InvalidOperationException($"Failed to sync customer profile with Stripe: {stripeEx.Message}", stripeEx);
                }
            }
            else
            {
                _logger.LogInformation("Performing local-only update for customer: {Email}", existingCustomer.Email);
                await SafeUpdateCustomerTableStorageAsync(existingCustomer);
            }

            _logger.LogInformation("Successfully updated customer profile: {Email}", existingCustomer.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer profile: {Email}", existingCustomer.Email);
            throw;
        }
    }

    /// <summary>
    /// Updates customer profile information and syncs with Stripe
    /// </summary>
    public async Task<bool> UpdateCustomerProfileAsync(
        string email,
        string fullName = null,
        string phone = null,
        string preferredCurrency = null,
        string defaultOriginAirport = null,
        Dictionary<string, string> additionalMetadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating customer profile: {Email}", email);

            var customer = await GetCustomerAsync(email, cancellationToken);
            if (customer == null)
            {
                _logger.LogWarning("Customer not found for profile update: {Email}", email);
                return false;
            }

            // Track what changed
            var changedFields = new List<string>();

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(fullName) && customer.FullName != fullName)
            {
                customer.FullName = fullName;
                changedFields.Add("FullName");
            }

            if (!string.IsNullOrWhiteSpace(phone) && customer.Phone != phone)
            {
                customer.Phone = phone;
                changedFields.Add("Phone");
            }

            if (!string.IsNullOrWhiteSpace(preferredCurrency) && customer.PreferredCurrency != preferredCurrency)
            {
                customer.PreferredCurrency = preferredCurrency;
                changedFields.Add("PreferredCurrency");
            }

            if (!string.IsNullOrWhiteSpace(defaultOriginAirport) && customer.DefaultOriginAirport != defaultOriginAirport)
            {
                customer.DefaultOriginAirport = defaultOriginAirport;
                changedFields.Add("DefaultOriginAirport");
            }

            // Update additional metadata
            if (additionalMetadata?.Any() == true)
            {
                foreach (var kvp in additionalMetadata)
                {
                    customer.SetMetadata(kvp.Key, kvp.Value);
                }
                changedFields.Add("Metadata");
            }

            if (!changedFields.Any())
            {
                _logger.LogInformation("No changes detected for customer profile: {Email}", email);
                return true;
            }

            // Update timestamps and tracking metadata
            customer.LastActiveAt = DateTime.UtcNow;
            customer.SetMetadata("last_profile_update", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            customer.SetMetadata("updated_fields", string.Join(",", changedFields));

            // Update locally first
            await SafeUpdateCustomerTableStorageAsync(customer);

            // Sync with Stripe only for fields that Stripe manages
            var stripeRelevantFields = changedFields.Where(f =>
                f == "FullName" || f == "Phone" || f == "Metadata").ToList();

            if (stripeRelevantFields.Any() && !string.IsNullOrWhiteSpace(customer.StripeCustomerId))
            {
                try
                {
                    // Create update request with only changed fields
                    var updateRequest = customer.ToUpdateRequest(stripeRelevantFields.ToArray());

                    await _stripeCustomerService.UpdateCustomerAsync(customer.StripeCustomerId, updateRequest, cancellationToken);

                    _logger.LogInformation("Successfully synced profile changes with Stripe for customer {Email}: {Fields}",
                        email, string.Join(", ", stripeRelevantFields));
                }
                catch (Exception stripeEx)
                {
                    _logger.LogError(stripeEx, "Failed to sync profile changes with Stripe for customer {Email}", email);

                    // Mark for retry
                    customer.SetMetadata("stripe_profile_sync_failed", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                    await SafeUpdateCustomerTableStorageAsync(customer);
                }
            }

            _logger.LogInformation("Updated customer profile for {Email}: {Fields}", email, string.Join(", ", changedFields));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer profile: {Email}", email);
            return false;
        }
    }


    /// <summary>
    /// Safely gets customer from local storage
    /// </summary>
    private async Task<Customer> GetLocalCustomerAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        try
        {
            var emailDomain = email.Split('@').LastOrDefault()?.ToLowerInvariant() ?? "unknown";
            var partitionKey = $"domain_{emailDomain}";
            var rowKey = email.ToLowerInvariant();

            return await _customerTableService.GetTableEntityAsync(partitionKey, rowKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting local customer: {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Handles different scenarios when syncing local and Stripe customer data
    /// </summary>
    private async Task<Customer> HandleCustomerSyncScenario(
        Customer localCustomer,
        CustomerResult stripeCustomer,
        string email,
        CancellationToken cancellationToken)
    {
        switch (stripeCustomer)
        {
            case null when localCustomer == null:
                _logger.LogInformation("Customer not found anywhere: {Email}", email);
                return null;
            case null:
                _logger.LogWarning("Customer exists locally but not in Stripe: {Email}", email);
                return localCustomer;
        }

        if (localCustomer == null)
        {
            _logger.LogWarning("Customer exists in Stripe but not locally: {Email}. Creating local record.", email);

            var newCustomer = _mapper.Map<Customer>(stripeCustomer);
            newCustomer.SetTableStorageKeys();

            Console.WriteLine(JsonSerializer.Serialize(newCustomer));
            await SafeInsertCustomerAsync(newCustomer);
            return newCustomer;
        }

        // Both exist - sync the data
        _logger.LogInformation("Syncing customer data: {Email}", email);

        var syncedCustomer = await SyncCustomerDataAsync(localCustomer, stripeCustomer, cancellationToken);
        return syncedCustomer;
    }

    /// <summary>
    /// Safely inserts customer with validation and error handling
    /// </summary>
    private async Task SafeInsertCustomerAsync(Customer customer)
    {
        try
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            customer.PrepareForStorage();

            if (!customer.IsValid(out var errors))
            {
                throw new ArgumentException($"Customer validation failed: {string.Join(", ", errors)}");
            }

            _logger.LogDebug("Inserting customer: Email={Email}, MetadataJson={MetadataJson}, PartitionKey={PartitionKey}, RowKey={RowKey}",
                customer.Email, customer.MetadataJson, customer.PartitionKey, customer.RowKey);

            await _customerTableService.InsertTableEntityAsync(customer);
            _logger.LogInformation("Customer inserted successfully: {Email}", customer.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting customer: {Email}", customer?.Email);
            throw;
        }
    }

    /// <summary>
    /// Safely updates customer with validation and error handling
    /// </summary>
    private async Task SafeUpdateCustomerTableStorageAsync(Customer customer)
    {
        try
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            customer.PrepareForStorage();

            if (!customer.IsValid(out var errors))
            {
                throw new ArgumentException($"Customer validation failed: {string.Join(", ", errors)}");
            }

            _logger.LogDebug("Updating customer: Email={Email}, MetadataJson={MetadataJson}, PartitionKey={PartitionKey}, RowKey={RowKey}",
                customer.Email, customer.MetadataJson, customer.PartitionKey, customer.RowKey);

            await _customerTableService.UpdateTableEntityAsync(customer);
            _logger.LogInformation("Customer updated successfully: {Email}", customer.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {Email}", customer?.Email);
            throw;
        }
    }

    /// <summary>
    /// Syncs Stripe customer data with local customer, preserving local-only fields
    /// </summary>
    private async Task<Customer> SyncCustomerDataAsync(
        Customer localCustomer,
        CustomerResult stripeCustomer,
        CancellationToken cancellationToken)
    {
        try
        {
            // Store local-only data
            var hasActiveSubscription = localCustomer.HasActiveSubscription;
            var subscriptionExpiresAt = localCustomer.SubscriptionExpiresAt;
            var currentSubscriptionId = localCustomer.CurrentSubscriptionId;
            var defaultOriginAirport = localCustomer.DefaultOriginAirport;
            var status = localCustomer.Status;
            var createdAt = localCustomer.CreatedAt;
            var localMetadata = localCustomer.Metadata ?? new Dictionary<string, string>();

            // Map Stripe data to existing customer (this will update Stripe-managed fields)
            _mapper.Map(stripeCustomer, localCustomer);

            // Restore local-only data
            localCustomer.HasActiveSubscription = hasActiveSubscription;
            localCustomer.SubscriptionExpiresAt = subscriptionExpiresAt;
            localCustomer.CurrentSubscriptionId = currentSubscriptionId;
            localCustomer.DefaultOriginAirport = defaultOriginAirport;
            localCustomer.Status = status;
            localCustomer.CreatedAt = createdAt;
            localCustomer.LastActiveAt = DateTime.UtcNow;

            // Merge metadata (preserve local metadata, add Stripe metadata)
            var mergedMetadata = localMetadata;
            var stripeMetadata = stripeCustomer.Metadata;
            foreach (var kvp in stripeMetadata)
            {
                if (!mergedMetadata.ContainsKey(kvp.Key))
                    mergedMetadata[kvp.Key] = kvp.Value;
            }
            mergedMetadata["last_sync"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            localCustomer.Metadata = mergedMetadata;

            localCustomer.SetTableStorageKeys();

            // Update in storage
            await SafeUpdateCustomerTableStorageAsync(localCustomer);

            _logger.LogInformation("Synced customer data table storage successfully: {Email}", localCustomer.Email);
            return localCustomer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing customer data: {Email}", localCustomer.Email);
            throw;
        }
    }

    private static void ValidateCreateRequest(CustomerSignupRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Name is required", nameof(request));
    }

    /// <summary>
    /// Checks if customer data is fresh enough to avoid Stripe sync
    /// </summary>
    private bool IsCustomerDataFresh(Customer customer, TimeSpan? customFreshnessThreshold = null)
    {
        if (customer?.LastActiveAt == null)
            return false;

        var freshnessThreshold = customFreshnessThreshold ?? TimeSpan.FromHours(1);
        var timeSinceLastUpdate = DateTime.UtcNow - customer.LastActiveAt.Value;

        var isFresh = timeSinceLastUpdate < freshnessThreshold;

        _logger.LogDebug("Customer data freshness check for {Email}: LastActive={LastActive}, Threshold={Threshold}, Fresh={IsFresh}",
            customer.Email, customer.LastActiveAt, freshnessThreshold, isFresh);

        return isFresh;
    }

    private async Task<Customer> GetCustomerFromStripeAndCache(string email, CancellationToken cancellationToken)
    {
        try
        {
            var stripeCustomer = await _stripeCustomerService.SearchCustomersAsync(email, cancellationToken);

            var newCustomer = _mapper.Map<Customer>(stripeCustomer);
            newCustomer.SetTableStorageKeys();
            newCustomer.PrepareForStorage();

            if (string.IsNullOrWhiteSpace(stripeCustomer.Email))
            {
                return null; // No customer found in Stripe
            }

            await SafeInsertCustomerAsync(newCustomer);

            _logger.LogInformation("Customer created from Stripe data: {Email}", email);
            return newCustomer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer from Stripe and caching: {Email}", email);
            throw;
        }
    }

    private async Task<Customer> RefreshCustomerFromStripe(Customer localCustomer, CancellationToken cancellationToken)
    {
        try
        {
            var stripeCustomer = await _stripeCustomerService.SearchCustomersAsync(localCustomer.Email, cancellationToken);

            // Sync the data
            var syncedCustomer = await SyncCustomerDataAsync(localCustomer, stripeCustomer, cancellationToken);
            syncedCustomer.SetMetadata("last_sync", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            syncedCustomer.SetMetadata("sync_status", "success");

            _logger.LogInformation("Successfully refreshed customer from Stripe: {Email}", localCustomer.Email);
            return syncedCustomer;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh customer from Stripe, using local data: {Email}", localCustomer.Email);

            // Mark the failed sync attempt but still return local data
            localCustomer.SetMetadata("last_sync_attempt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            localCustomer.SetMetadata("sync_status", "failed");
            localCustomer.SetMetadata("sync_error", ex.Message);

            try
            {
                await SafeUpdateCustomerTableStorageAsync(localCustomer);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update sync metadata for customer: {Email}", localCustomer.Email);
            }

            return localCustomer;
        }
    }
}