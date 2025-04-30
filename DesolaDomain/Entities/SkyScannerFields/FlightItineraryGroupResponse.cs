namespace DesolaDomain.Entities.SkyScannerFields;

public class FlightItineraryGroupResponse
{
    public decimal TotalPrice { get; set; }
    public string PriceCurrency { get; set; }
    public FlightItineraryResponse Departure { get; set; }
    public FlightItineraryResponse Return { get; set; }

}