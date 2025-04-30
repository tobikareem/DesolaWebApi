namespace DesolaDomain.Entities.SkyScannerFields;

public class FlightItineraryResponse
{
    public string TotalDuration { get; set; }
    public int NumberOfStopOver { get; set; }
    public List<FlightSegmentResponse> Segments { get; set; } = new();
}