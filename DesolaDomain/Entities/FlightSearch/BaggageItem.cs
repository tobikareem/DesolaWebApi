namespace DesolaDomain.Entities.FlightSearch;

public class BaggageItem
{
    public int Quantity { get; set; }
    public decimal WeightKg { get; set; }
    public string Dimensions { get; set; }
    public string Description { get; set; }
    public decimal? AdditionalCost { get; set; }
}