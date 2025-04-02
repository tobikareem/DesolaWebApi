using CsvHelper.Configuration;

namespace DesolaDomain.Model;

public sealed class AirportCsvMap : ClassMap<Airport>
{
    public AirportCsvMap()
    {
        Map(m => m.Name).Name("name");
        Map(m => m.City).Name("municipality");
        Map(m => m.Code).Name("iata_code");
        Map(m => m.AirportType).Name("type");
        Map(m => m.CountryCode).Name("iso_country");
        Map(m => m.Region).Name("iso_region");
    }
}