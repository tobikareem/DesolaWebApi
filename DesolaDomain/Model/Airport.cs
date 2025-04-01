namespace DesolaDomain.Model;

public class Airport
{
    public string Name { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Code { get; set; } = default!; // IATA code
    public string AirportType { get; set; } = default!;
    public string CountryCode { get; set; } = default!;
    public string Region { get; set; } = default!;
}