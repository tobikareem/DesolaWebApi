namespace DesolaDomain.Entities.FlightSearch;

public class FareRules
{
    public bool IsRefundable { get; set; }
    public bool IsChangeable { get; set; }
    public decimal? ChangeFee { get; set; }
    public decimal? CancellationFee { get; set; }
    public string FareExpirationDate { get; set; }
    public List<string> FareConditions { get; set; }
    public string MinimumStay { get; set; }
    public string MaximumStay { get; set; }
}