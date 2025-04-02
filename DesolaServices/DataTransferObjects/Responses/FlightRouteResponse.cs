namespace DesolaServices.DataTransferObjects.Responses;

public class FlightRouteResponse
{
    public string Type { get; set; }
    public string Subtype { get; set; }
    public string Name { get; set; }
    public string IataCode { get; set; }
    public GeoCodeResponse GeoCode { get; set; }
    public AddressResponse Address { get; set; }
    public string TimeZone { get; set; }
}