using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class DatumPrice
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("total")]
    public string Total { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; }

    [JsonPropertyName("fees")]
    public List<Fee> Fees { get; set; }

    [JsonPropertyName("grandTotal")]
    public string GrandTotal { get; set; }
}