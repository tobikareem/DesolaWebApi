using CsvHelper.Configuration;

namespace DesolaDomain.Model;

public class Airport
{
    public string Name { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Code { get; set; } = default!; // IATA code
    public string AirportType { get; set; } = default!;

}


public sealed class AirportCsvMap : ClassMap<Airport>
{
    public AirportCsvMap()
    {
        Map(m => m.Name).Name("name");
        Map(m => m.City).Name("municipality");
        Map(m => m.Code).Name("iata_code");
        Map(m => m.AirportType).Name("type");
    }
}