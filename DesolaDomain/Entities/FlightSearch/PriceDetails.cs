namespace DesolaDomain.Entities.FlightSearch;

public class PriceDetails
{
    // Total price in the requested currency
    public decimal TotalPrice { get; set; }

    // Currency code (e.g., USD, EUR)
    public string Currency { get; set; }

    // Base fare without taxes and fees
    public decimal BaseFare { get; set; }

    // Taxes and fees breakdown
    public decimal Taxes { get; set; }
    public decimal Fees { get; set; }

    // Price per passenger type
    public Dictionary<string, decimal> PerPassengerPrices { get; set; } // e.g., "adult": 450.00

    // Price comparison metrics
    public decimal AveragePriceForRoute { get; set; }
    public decimal PriceComparisonPercentage { get; set; } // How much below/above average
}