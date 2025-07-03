using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.User;

public class Customer : ITableEntity
{
    public Customer()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;
        ETag = ETag.All;
        Timestamp = DateTimeOffset.UtcNow;
        Status = CustomerStatus.Active;
    }

    /// <summary>
    /// Unique identifier for the customer
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Customer ID as string (for compatibility)
    /// </summary>
    public string CustomerId
    {
        get => Id.ToString();
        set => Id = Guid.TryParse(value, out var guid) ? guid : Guid.NewGuid();
    }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// The Stripe Customer ID for payment processing
    /// </summary>
    public string StripeCustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the customer has an active subscription
    /// </summary>
    public bool HasActiveSubscription { get; set; } = false;

    /// <summary>
    /// When the current subscription expires (if any)
    /// </summary>
    public DateTime? SubscriptionExpiresAt { get; set; }

    /// <summary>
    /// The customer's current subscription plan ID
    /// </summary>
    public string CurrentSubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// Customer's preferred currency
    /// </summary>
    public string PreferredCurrency { get; set; } = "USD";

    /// <summary>
    /// Customer's default origin airport for flight searches
    /// </summary>
    public string DefaultOriginAirport { get; set; } = string.Empty;

    /// <summary>
    /// When the customer account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last time customer was active
    /// </summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    /// Customer status (Active, Suspended, Deleted)
    /// </summary>
    public CustomerStatus Status { get; set; }

    /// <summary>
    /// Metadata stored as JSON string for Azure Table Storage compatibility
    /// </summary>
    public string MetadataJson { get; set; } = "{}";

    // Azure Table Storage required properties
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    /// <summary>
    /// Metadata dictionary that automatically serializes to/from JSON
    /// </summary>
    [JsonIgnore]                    
    [IgnoreDataMember]              
    [Newtonsoft.Json.JsonIgnore]

    public Dictionary<string, string> Metadata
    {
        get
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MetadataJson))
                    return new Dictionary<string, string>();

                return JsonSerializer.Deserialize<Dictionary<string, string>>(MetadataJson)
                       ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>();
            }
        }
        set
        {
            try
            {
                MetadataJson = value != null
                    ? JsonSerializer.Serialize(value)
                    : "{}";
            }
            catch (JsonException)
            {
                MetadataJson = "{}";
            }
        }
    }

    // Helper methods for metadata management
    public void SetMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var metadata = Metadata;
        metadata[key] = value ?? string.Empty;
        Metadata = metadata;
    }

    public string GetMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        var metadata = Metadata;
        return metadata.TryGetValue(key, out var value) ? value : null;
    }

    public bool HasMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;

        var metadata = Metadata;
        return metadata.ContainsKey(key);
    }

    public void RemoveMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var metadata = Metadata;
        metadata.Remove(key);
        Metadata = metadata;
    }

    public void ClearMetadata()
    {
        Metadata = new Dictionary<string, string>();
    }

    /// <summary>
    /// Sets the partition and row keys for Azure Table Storage
    /// </summary>
    public void SetTableStorageKeys()
    {
        if (!string.IsNullOrWhiteSpace(Email))
        {
            var emailDomain = Email.Split('@').LastOrDefault()?.ToLowerInvariant() ?? "unknown";
            PartitionKey = $"domain_{emailDomain}";
            RowKey = Email.ToLowerInvariant();
        }
        else
        {
            PartitionKey = "unknown";
            RowKey = Id.ToString();
        }
    }

    /// <summary>
    /// Validates the customer entity
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(Email))
            errors.Add("Invalid email format");

        if (string.IsNullOrWhiteSpace(FullName))
            errors.Add("Full name is required");

        if (string.IsNullOrWhiteSpace(PartitionKey))
            errors.Add("PartitionKey is required");

        if (string.IsNullOrWhiteSpace(RowKey))
            errors.Add("RowKey is required");

        return errors.Count == 0;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var emailAddress = new System.Net.Mail.MailAddress(email);
            return emailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public void PrepareForStorage()
    {
        if (Metadata != null)
        {
            try
            {
                // This will trigger the setter which serializes to MetadataJson
                var temp = Metadata;
                Metadata = temp;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preparing metadata for storage: {ex.Message}");
                MetadataJson = "{}";
            }
        }
        
        SetTableStorageKeys();

        // Ensure timestamps are set
        if (CreatedAt == default)
            CreatedAt = DateTime.UtcNow;

        LastActiveAt = DateTime.UtcNow;
    }
}

public enum CustomerStatus
{
    Active,
    Suspended,
    Deleted
}