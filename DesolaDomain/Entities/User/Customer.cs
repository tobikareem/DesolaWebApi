using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace DesolaDomain.Entities.User;

public class Customer: ITableEntity
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; }

    /// <summary>
    /// The Stripe Customer ID for payment processing
    /// </summary>
    public string StripeCustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the customer has an active subscription
    /// </summary>
    public bool HasActiveSubscription { get; set; }

    /// <summary>
    /// When the current subscription expires (if any)
    /// </summary>
    public DateTime? SubscriptionExpiresAt { get; set; }

    /// <summary>
    /// The customer's current subscription plan ID
    /// </summary>
    public string CurrentSubscriptionId { get; set; }

    /// <summary>
    /// Customer's preferred currency
    /// </summary>
    public string PreferredCurrency { get; set; } = "USD";

    /// <summary>
    /// Customer's default origin airport for flight searches
    /// </summary>
    public string DefaultOriginAirport { get; set; }

    /// <summary>
    /// When the customer account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time customer was active
    /// </summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    /// Customer status (Active, Suspended, Deleted)
    /// </summary>
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    public Dictionary<string, string> Metadata { get; set; } = new();

    // Azure Table Storage required properties
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string CustomerId { get; set; }
}

public enum CustomerStatus
{
    Active,
    Suspended,
    Deleted
}