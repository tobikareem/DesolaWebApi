

namespace DesolaDomain.Entities.SkyScannerFields;

public class FlightTravelerPricingResponse
{
    public string FareOption;
    public string TravelerId { get; set; }
    public string TravelerType { get; set; }
    public string PriceCurrency { get; set; }
    public string TotalPrice { get; set; }
}