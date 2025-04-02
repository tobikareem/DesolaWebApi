namespace DesolaDomain.Entities.AmadeusFields.Advanced;

public class ConnectionRestriction
{
    public int? MaxNumberOfConnections { get; set; }
    public bool NonStopPreferred { get; set; }
    public bool AirportChangeAllowed { get; set; }
    public bool TechnicalStopsAllowed { get; set; }
}