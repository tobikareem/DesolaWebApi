using System.Text.Json.Serialization;

namespace DesolaServices.DataTransferObjects.Requests;

public class FlightFilters
{
    [JsonPropertyName("cabinRestrictions")]
    public List<CabinRestriction> CabinRestrictions { get; set; }

    [JsonPropertyName("carrierRestrictions")]
    public CarrierRestrictions CarrierRestrictions { get; set; }
}