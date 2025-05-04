
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.GoogleFields.Request;

/// <summary>
/// Represents flight search result sorting options
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FlightSortOption
{
    /// <summary>
    /// Sort by top flights (default)
    /// </summary>
    [EnumMember(Value = "1")]
    [Description("Top flights")]
    TopFlights = 1,

    /// <summary>
    /// Sort by price (lowest to highest)
    /// </summary>
    [EnumMember(Value = "2")]
    [Description("Price")]
    Price = 2,

    /// <summary>
    /// Sort by departure time (earliest to latest)
    /// </summary>
    [EnumMember(Value = "3")]
    [Description("Departure time")]
    DepartureTime = 3,

    /// <summary>
    /// Sort by arrival time (earliest to latest)
    /// </summary>
    [EnumMember(Value = "4")]
    [Description("Arrival time")]
    ArrivalTime = 4,

    /// <summary>
    /// Sort by flight duration (shortest to longest)
    /// </summary>
    [EnumMember(Value = "5")]
    [Description("Duration")]
    Duration = 5,

    /// <summary>
    /// Sort by emissions (lowest to highest)
    /// </summary>
    [EnumMember(Value = "6")]
    [Description("Emissions")]
    Emissions = 6
}
