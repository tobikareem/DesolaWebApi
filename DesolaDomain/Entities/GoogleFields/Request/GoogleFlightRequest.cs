namespace DesolaDomain.Entities.GoogleFields.Request;

public class GoogleFlightRequest
{
    public string DepartureId { get; set; }
    public string ArrivalId { get; set; }
    public string DepartureDate { get; set; }
    public string ArrivalDate { get; set; }
    public int CabinClass { get; set; }
    public int StopOver { get; set; }
    public int MaxPrice { get; set; }
    public int FlightSortOption { get; set; }
    public int Adults { get; set; }
    public bool IsOneWay { get; set; }
}