namespace DesolaDomain.Entities.FlightSearch;

public class ValueScores
{
    public decimal OverallValue { get; set; } // 0-100 composite score
    public decimal PriceScore { get; set; } // How good is the price (0-100)
    public decimal ConvenienceScore { get; set; } // Based on duration, stops, time (0-100)
    public decimal ComfortScore { get; set; } // Based on aircraft, cabin, amenities (0-100)
    public decimal ReliabilityScore { get; set; } // Based on airline performance (0-100)
}