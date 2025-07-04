using Azure;
using Azure.Data.Tables;

namespace DesolaDomain.Entities.Payment;

public class DesolaPriceDetail : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string StripePriceId { get; set; } = string.Empty;
    public string StripeProductId { get; set; } = string.Empty;
    public decimal UnitAmountInCents { get; set; } // In cents
    public string Currency { get; set; } = "usd";
    public string Nickname { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string BillingInterval { get; set; } = string.Empty; // month, year
    public int IntervalCount { get; set; } = 1;
    public string UsageType { get; set; } = "licensed"; // licensed, metered

    public decimal AmountInDollars => UnitAmountInCents / 100m;
    public string FormattedPrice => $"${AmountInDollars:F2}/{BillingInterval}";
    public bool IsTrialEligible { get; set; } = true;
    public int TrialDays { get; set; } = 0;
    public string PromotionalTag { get; set; } = string.Empty;
    public string Metadata { get; set; } = "{}";
    
    public DateTime LastSyncedFromStripe { get; set; }
    public string SyncStatus { get; set; } = "synced";

}