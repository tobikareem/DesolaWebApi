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

public class GeoCodeResponse
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class AddressResponse
{
    public string CountryName { get; set; }
    public string CountryCode { get; set; }
    public string StateCode { get; set; }
    public string RegionCode { get; set; }
}