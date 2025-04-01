using System.Text.Json.Serialization;

namespace DesolaServices.DataTransferObjects.Requests;

public class CabinRestriction
{
    [JsonPropertyName("cabin")]
    public string Cabin { get; set; }

    [JsonPropertyName("coverage")]
    public string Coverage { get; set; }

    [JsonPropertyName("originDestinationIds")]
    public List<string> OriginDestinationIds { get; set; }
}