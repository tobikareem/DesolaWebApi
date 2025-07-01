namespace DesolaServices.DataTransferObjects.Responses;

public class ClickHistoryItem
{
    public string Id { get; set; }
    public string ClickedAt { get; set; }
    public string FlightOffer { get; set; }
    public string FlightOrigin { get; set; }
    public string FlightDestination { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}