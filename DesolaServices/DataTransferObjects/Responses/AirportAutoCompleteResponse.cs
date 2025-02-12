using DesolaDomain.Model;

namespace DesolaServices.DataTransferObjects.Responses;

public class AirportAutoCompleteResponse : Airport
{
    public string AirportId { get; set; }

    public string Country { get; set; }
    public string EntityId { get; set; }

    public string Identity { get; set; }
}