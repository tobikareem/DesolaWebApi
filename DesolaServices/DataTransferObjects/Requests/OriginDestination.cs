using System.Text.Json.Serialization;

namespace DesolaServices.DataTransferObjects.Requests;

public class OriginDestination
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("originLocationCode")]
    public string OriginLocationCode { get; set; }

    [JsonPropertyName("destinationLocationCode")]
    public string DestinationLocationCode { get; set; }

    [JsonPropertyName("departureDateTimeRange")]
    public DepartureDateTimeRange DepartureDateTimeRange { get; set; }
}