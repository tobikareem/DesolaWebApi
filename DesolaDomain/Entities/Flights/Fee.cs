using System.Text.Json.Serialization;
using DesolaDomain.Enums;

namespace DesolaDomain.Entities.Flights;

public class Fee
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}