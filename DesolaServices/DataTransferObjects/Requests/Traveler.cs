using System.Text.Json.Serialization;

namespace DesolaServices.DataTransferObjects.Requests;

public class Traveler
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("travelerType")]
    public string TravelerType { get; set; }

    [JsonPropertyName("fareOptions")]
    public List<string> FareOptions { get; set; }
}