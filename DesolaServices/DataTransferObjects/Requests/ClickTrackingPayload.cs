using DesolaDomain.Entities.FlightSearch;

namespace DesolaServices.DataTransferObjects.Requests;

public class ClickTrackingPayload
{
    public string UserId { get; set; }
    public UnifiedFlightOffer  UnifiedFlightOffer { get; set; }
    public string ClickedAt { get; set; }
}