using CaptainPayment.Core.Models;

namespace DesolaServices.Commands.Requests;

public class UpdateProductDisplayRequest
{
    public int? DisplayOrder { get; set; }
    public bool? IsRecommended { get; set; }
    public bool? IsActive { get; set; }
    public string Features { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class UpdatePriceDisplayRequest
{
    public bool? IsTrialEligible { get; set; }
    public int? TrialDays { get; set; }
    public string PromotionalTag { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}

public class DesolaSubscriptionPlanRequest
{
    public string PlanName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long AmountInCents { get; set; }
    public StripeInterval Interval { get; set; }
    public string Currency { get; set; } = "usd";
}