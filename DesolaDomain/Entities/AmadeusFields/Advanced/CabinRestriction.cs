namespace DesolaDomain.Entities.AmadeusFields.Advanced;

public class CabinRestriction
{
    public string Cabin { get; set; }
    public List<string> OriginDestinationIds { get; set; }
    public string Coverage { get; set; } // MOST_SEGMENTS, AT_LEAST_ONE_SEGMENT, ALL_SEGMENTS
}