namespace DesolaDomain.Entities.FlightSearch;

public class SearchStats
{
    public int TotalResults { get; set; }
    public int FilteredResults { get; set; }
    public Dictionary<string, int> ResultsBySource { get; set; } // e.g., "Amadeus": 120
    public Dictionary<string, int> ResultsByAirline { get; set; }
    public int SpecialOpportunitiesFound { get; set; }
    public Dictionary<string, int> OpportunitiesByType { get; set; }
    public int ApiCallsMade { get; set; }
    public int CacheHits { get; set; }
    public double SearchTimeSeconds { get; set; }
}