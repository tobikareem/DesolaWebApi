namespace DesolaServices.DataTransferObjects.Responses;

public class FlightSearchResponse
{
    public string FlightFrom { get; set; }
    public string FlightTo { get; set; }

    public DateTime DepartureDateTime { get; set; }

    public DateTime ArrivalDateTime { get; set; }

    public string FlightNumber { get; set; }

    public string Airline { get; set; }

    public string Aircraft { get; set; }

    public decimal TotalPrice { get; set; }

    public string FlightDuration { get; set; }

    public int NumberOfStopOver { get; set; }

    public string PriceCurrency { get; set; }

}