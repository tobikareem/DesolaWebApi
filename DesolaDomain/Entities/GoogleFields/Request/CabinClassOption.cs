using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.GoogleFields.Request;


/// <summary>
/// Represents flight cabin class options
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CabinClassOption
{
    /// <summary>
    /// Economy class (default)
    /// </summary>
    [EnumMember(Value = "1")]
    [Description("Economy")]
    Economy = 1,

    /// <summary>
    /// Premium economy class
    /// </summary>
    [EnumMember(Value = "2")]
    [Description("Premium economy")]
    PremiumEconomy = 2,

    /// <summary>
    /// Business class
    /// </summary>
    [EnumMember(Value = "3")]
    [Description("Business")]
    Business = 3,

    /// <summary>
    /// First class
    /// </summary>
    [EnumMember(Value = "4")]
    [Description("First")]
    First = 4
}