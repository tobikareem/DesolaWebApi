using System.Text.Json.Serialization;

namespace DesolaServices.DataTransferObjects.Requests;

public class CarrierRestrictions
{
    [JsonPropertyName("excludedCarrierCodes")]
    public List<string> ExcludedCarrierCodes { get; set; }
}