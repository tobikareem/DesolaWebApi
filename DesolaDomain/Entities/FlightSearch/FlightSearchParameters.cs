namespace DesolaDomain.Entities.FlightSearch;

public class FlightSearchParameters
{
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public int Infants { get; set; }
    public string CabinClass { get; set; }
    public List<string> PreferredAirlines { get; set; }
    public int? MaxStops { get; set; }
    public string Currency { get; set; }
}