namespace DesolaDomain.Entities.FlightSearch;

public class SeatDetails
{
    public int SeatsRemaining { get; set; }
    public string SeatPitch { get; set; }
    public string SeatWidth { get; set; }
    public bool HasPowerOutlet { get; set; }
    public bool HasWifi { get; set; }
    public bool HasEntertainment { get; set; }
}