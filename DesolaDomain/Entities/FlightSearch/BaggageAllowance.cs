namespace DesolaDomain.Entities.FlightSearch;

public class BaggageAllowance
{
    public List<BaggageItem> CarryOn { get; set; }
    public List<BaggageItem> CheckedBags { get; set; }
    public string BaggageDisclaimer { get; set; }
}