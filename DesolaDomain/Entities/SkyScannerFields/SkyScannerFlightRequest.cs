using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.SkyScannerFields;

public class SkyScannerFlightRequest
{
    public string FromEntityId { get; set; }
    public string ToEntityId { get; set; }
    public string DepartDate { get; set; }
    public string ReturnDate { get; set; }
    public string Market { get; set; } = "US";
    public string Currency { get; set; } = "USD";
    public string Stops { get; set; } = "direct,1stop";
    public int Adults { get; set; } = 1;
    public int Infants { get; set; } = 0;
    public string CabinClass { get; set; }
    public string SortBy { get; set; }
    public string SortOrder { get; set; }
    public bool IsOneWay { get; set; }
}

/// <summary>
/// Represents flight cabin class options
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CabinClassOption
{
    /// <summary>
    /// Economy class (default)
    /// </summary>
    [EnumMember(Value = "economy")]
    [Description("Economy")]
    Economy = 1,

    /// <summary>
    /// Premium economy class
    /// </summary>
    [EnumMember(Value = "premium_economy")]
    [Description("Premium economy")]
    PremiumEconomy = 2,

    /// <summary>
    /// Business class
    /// </summary>
    [EnumMember(Value = "business")]
    [Description("Business")]
    Business = 3,

    /// <summary>
    /// First class
    /// </summary>
    [EnumMember(Value = "first")]
    [Description("First")]
    First = 4
}