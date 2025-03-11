namespace DesolaServices.DataTransferObjects.Requests;

public class UserTravelPreferenceRequest
{
    public string OriginAirport { get; set; }
    public string DestinationAirport { get; set; }
    public string TravelClass { get; set; }
    public string StopOvers { get; set; }
    public string UserId { get; set; }
}