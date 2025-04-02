namespace DesolaDomain.Entities.AmadeusFields.Advanced;

public class PricingOptions
{
    public bool? IncludedCheckedBagsOnly { get; set; }
    public bool? RefundableFare { get; set; }
    public bool? NoRestrictionFare { get; set; }
    public bool? NoPenaltyFare { get; set; }
}