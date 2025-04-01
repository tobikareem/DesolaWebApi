using System.Text.Json.Serialization;

namespace DesolaServices.DataTransferObjects.Requests;

public class DepartureDateTimeRange
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; }
}