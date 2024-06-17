namespace DesolaServices.DataTransferObjects.Requests;

public class SkyScannerFlightRequest
{
    public string FromEntityId { get; set; }
    public string ToEntityId { get; set; }
    public string DepartDate { get; set; }
    public string ReturnDate { get; set; }
    public string Market { get; set; } = "US";
    public string Currency { get; set; } = "USD";
    public string Stops { get; set; } = "direct,1stop";
    public int Adults { get; set; } = 1;
    public int Infants { get; set; } = 0;
    public string CabinClass { get; set; }
    public string SortBy { get; set; }
    public string SortOrder { get; set; }
}