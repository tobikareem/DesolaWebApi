namespace DesolaServices.DataTransferObjects.Responses;

public class FlightSegmentResponse
{
    public string FlightFrom { get; set; }
    public string FlightTo { get; set; }
    public string DepartureDateTime { get; set; }
    public string ArrivalDateTime { get; set; }
    public string FlightNumber { get; set; }
    public string Airline { get; set; }
    public string Aircraft { get; set; }
    public string AircraftPhotoLink { get; set; }
    public string FlightDuration { get; set; }
}