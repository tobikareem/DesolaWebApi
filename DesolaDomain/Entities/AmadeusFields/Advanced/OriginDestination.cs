namespace DesolaDomain.Entities.AmadeusFields.Advanced;

public class OriginDestination
{
    public string Id { get; set; }
    public string OriginLocationCode { get; set; }
    public string DestinationLocationCode { get; set; }
    public DateTime DepartureDate { get; set; }
    public TimeSpan? DepartureTime { get; set; }
    public List<string> IncludedConnectionPoints { get; set; }
    public List<string> ExcludedConnectionPoints { get; set; }
}