namespace DesolaDomain.Entities.FlightSearch;

public class SpecialOpportunity
{
    public string Type { get; set; } // e.g., "HiddenCity", "MultiCarrier", "PriceAnomaly"
    public string Description { get; set; }
    public decimal SavingsAmount { get; set; }
    public decimal SavingsPercentage { get; set; }
    public List<string> Warnings { get; set; } // e.g., "Baggage cannot be checked through"
    public string RecommendedBookingMethod { get; set; }
}