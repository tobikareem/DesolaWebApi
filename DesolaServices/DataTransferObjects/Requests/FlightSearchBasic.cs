namespace DesolaServices.DataTransferObjects.Requests;

public class FlightSearchBasic
{
    public required string Origin { get; set; }
    public required string Destination { get; set; }
    public required DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Adults { get; set; } = 1;
    public int MaxResults { get; set; } = 5;
}