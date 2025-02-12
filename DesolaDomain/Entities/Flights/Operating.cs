using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Operating
{
    [JsonPropertyName("carrierCode")]
    public string CarrierCode { get; set; }
}