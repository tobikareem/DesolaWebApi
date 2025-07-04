using Azure;
using Azure.Data.Tables;

namespace DesolaDomain.Entities.Payment;

public class DesolaProductDetail : ITableEntity
{
    public string PartitionKey { get; set; } = "DesolaProduct";
    public string RowKey { get; set; } = string.Empty; // This should be the stripe product ID 
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // Stripe Product Information
    public string StripeProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string ProductType { get; set; } = "subscription"; 

    public string Metadata { get; set; } = "{}";
    public DateTime LastSyncedFromStripe { get; set; }
    public string SyncStatus { get; set; } = "synced";

}