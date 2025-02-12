using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.DataTransferObjects.Requests;

public class FlightSearchBasicRequest : IRequest<Dictionary<string, FlightItineraryGroupResponse>>
{
    public required string Origin { get; set; }
    public  string Destination { get; set; }
    public required DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Adults { get; set; } = 1;
    public int MaxResults { get; set; } = 5;
    public string SortBy { get; set; } 
    public string SortOrder { get; set; }
    public string Stops { get; set; }
    public string Infants { get; set; }
    public string CabinClass { get; set; }
}