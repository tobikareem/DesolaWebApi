namespace DesolaDomain.Entities.Payment;

public class DesolaSubscriptionPlan
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Features { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
    public int DisplayOrder { get; set; }
    
    public string PriceId { get; set; } = string.Empty;
    public decimal AmountInCents { get; set; }
    public decimal AmountInDollars => AmountInCents / 100m;
    public string Currency { get; set; } = string.Empty;
    public string BillingInterval { get; set; } = string.Empty;
    public string FormattedPrice { get; set; } = string.Empty;
    public string PromotionalTag { get; set; } = string.Empty;
    public bool IsTrialEligible { get; set; }
    public int TrialDays { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}