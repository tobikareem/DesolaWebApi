using CaptainPayment.Core.Models;
using DesolaDomain.Entities.User;

namespace DesolaServices.Utility;

public static class CustomerUpdateMappingExtensions
{
    /// <summary>
    /// Creates an UpdateCustomerRequest with only specified fields populated
    /// </summary>
    public static UpdateCustomerRequest ToUpdateRequest(this Customer customer, params string[] fieldsToUpdate)
    {
        var request = new UpdateCustomerRequest();

        var fieldsSet = new HashSet<string>(fieldsToUpdate, StringComparer.OrdinalIgnoreCase);

        if (fieldsSet.Contains("Email") || fieldsSet.Contains("All"))
            request.Email = customer.Email;

        if (fieldsSet.Contains("Name") || fieldsSet.Contains("FullName") || fieldsSet.Contains("All"))
            request.Name = customer.FullName;

        if (fieldsSet.Contains("Phone") || fieldsSet.Contains("All"))
            request.Phone = customer.Phone ?? string.Empty;

        if (fieldsSet.Contains("Description") || fieldsSet.Contains("All"))
            request.Description = CreateCustomerDescription(customer);

        if (fieldsSet.Contains("Metadata") || fieldsSet.Contains("All"))
            request.Metadata = CreateStripeMetadata(customer);

        // Address fields are not supported yet, but we'll provide empty ones
        if (fieldsSet.Contains("Address") || fieldsSet.Contains("All"))
        {
            request.Address = new CustomerAddress();
            request.ShippingAddress = new CustomerAddress();
        }

        return request;
    }

    public static UpdateCustomerRequest ToSubscriptionUpdateRequest(this Customer customer)
    {
        return new UpdateCustomerRequest
        {
            Email = customer.Email,
            Name = customer.FullName,
            Phone = customer.Phone ?? string.Empty,
            Description = CreateCustomerDescription(customer),
            Metadata = CreateSubscriptionMetadata(customer),
            Address = new CustomerAddress(),
            ShippingAddress = new CustomerAddress(),
            DefaultPaymentMethodId = string.Empty
        };
    }

    private static string CreateCustomerDescription(Customer customer)
    {
        var parts = new List<string> { "Desola Flights Customer" };

        if (customer.HasActiveSubscription)
            parts.Add("Active Subscriber");

        if (!string.IsNullOrWhiteSpace(customer.DefaultOriginAirport))
            parts.Add($"Home Airport: {customer.DefaultOriginAirport}");

        parts.Add($"Member since {customer.CreatedAt:yyyy-MM-dd}");

        return string.Join(" | ", parts);
    }

    private static Dictionary<string, string> CreateStripeMetadata(Customer customer)
    {
        var metadata = customer.Metadata ?? new Dictionary<string, string>();

        // Add system metadata
        metadata["customer_id"] = customer.Id.ToString();
        metadata["status"] = customer.Status.ToString();
        metadata["has_active_subscription"] = customer.HasActiveSubscription.ToString().ToLowerInvariant();
        metadata["last_updated"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        if (customer.HasActiveSubscription && customer.SubscriptionExpiresAt.HasValue)
            metadata["subscription_expires"] = customer.SubscriptionExpiresAt.Value.ToString("yyyy-MM-dd");

        if (!string.IsNullOrWhiteSpace(customer.CurrentSubscriptionId))
            metadata["current_subscription_id"] = customer.CurrentSubscriptionId;

        return metadata;
    }

    private static Dictionary<string, string> CreateSubscriptionMetadata(Customer customer)
    {
        var metadata = new Dictionary<string, string>
        {
            ["customer_id"] = customer.Id.ToString(),
            ["has_active_subscription"] = customer.HasActiveSubscription.ToString().ToLowerInvariant(),
            ["subscription_status"] = customer.HasActiveSubscription ? "active" : "inactive",
            ["last_subscription_update"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        if (customer.HasActiveSubscription && customer.SubscriptionExpiresAt.HasValue)
            metadata["subscription_expires"] = customer.SubscriptionExpiresAt.Value.ToString("yyyy-MM-dd");

        if (!string.IsNullOrWhiteSpace(customer.CurrentSubscriptionId))
            metadata["current_subscription_id"] = customer.CurrentSubscriptionId;

        // Preserve existing metadata that's relevant to subscriptions
        var existingMetadata = customer.Metadata ?? new Dictionary<string, string>();
        foreach (var kvp in existingMetadata)
        {
            if (kvp.Key.Contains("subscription", StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Contains("billing", StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Contains("payment", StringComparison.OrdinalIgnoreCase))
            {
                metadata[kvp.Key] = kvp.Value;
            }
        }

        return metadata;
    }
}