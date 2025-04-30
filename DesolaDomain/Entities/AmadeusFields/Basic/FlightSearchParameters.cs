namespace DesolaDomain.Entities.AmadeusFields.Basic;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public class FlightSearchParameters : IValidatableObject
{
    [JsonPropertyName("originLocationCode")]
    [Required(ErrorMessage = "Origin location is required.")]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Origin must be a valid 3-letter IATA code.")]
    public string Origin { get; set; }

    [JsonPropertyName("destinationLocationCode")]
    [Required(ErrorMessage = "Destination location is required.")]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Destination must be a valid 3-letter IATA code.")]

    public string Destination { get; set; }

    [JsonPropertyName("departureDate")]
    [Required(ErrorMessage = "Departure date is required.")]
    public DateTime DepartureDate { get; set; }

    [JsonPropertyName("returnDate")]
    public DateTime? ReturnDate { get; set; }

    [JsonPropertyName("adults")]
    [Range(1, int.MaxValue, ErrorMessage = "At least one adult is required.")]
    public int Adults { get; set; }

    [JsonPropertyName("children")]
    [Range(0, int.MaxValue)]
    public int Children { get; set; } = 0;

    [JsonPropertyName("infants")]
    [Range(0, int.MaxValue)]
    public int Infants { get; set; } = 0;

    private string _cabinClass;
    [JsonPropertyName("travelClass")]
    [Required(ErrorMessage = "Travel class is required.")]
    public string CabinClass
    {
        get => _cabinClass;
        set => _cabinClass = value?.ToUpperInvariant();
    }

    [JsonPropertyName("currencyCode")]
    [Required(ErrorMessage = "Currency code is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be a 3-letter code.")]
    public string CurrencyCode { get; set; } = "USD";

    [JsonPropertyName("maxPrice")]
    [Range(0, int.MaxValue, ErrorMessage = "Max price must be a non-negative number.")]
    public int? MaxPrice { get; set; }

    [JsonPropertyName("max")]
    [Range(1, int.MaxValue, ErrorMessage = "Max results must be at least 1.")]
    public int MaxResults { get; set; } = 250;

    [JsonPropertyName("nonStop")]
    public bool NonStop { get; set; } = false;

    [JsonPropertyName("includedAirlineCodes")]
    public List<string> IncludedAirlineCodes { get; set; } = new();

    [JsonPropertyName("excludedAirlineCodes")]
    public List<string> ExcludedAirlineCodes { get; set; } = new();

    [JsonPropertyName("sortBy")]
    public string SortBy { get; set; }

    [JsonPropertyName("sortOrder")]
    public string SortOrder { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DepartureDate.Date < DateTime.Today)
        {
            yield return new ValidationResult("Departure date must not be in the past.", new[] { nameof(DepartureDate) });
        }
        
        if (ReturnDate.HasValue && ReturnDate.Value.Date < DepartureDate.Date)
        {
            yield return new ValidationResult("Return date must be on or after the departure date.", new[] { nameof(ReturnDate) });
        }
        
        var validClasses = new List<string> { "ECONOMY", "PREMIUM_ECONOMY", "BUSINESS", "FIRST" };
        if (!string.IsNullOrEmpty(CabinClass) && !validClasses.Contains(CabinClass))
        {
            yield return new ValidationResult($"Invalid travel class. Allowed values are: {string.Join(", ", validClasses)}.", new[] { nameof(CabinClass) });
        }
        
        if (!string.IsNullOrEmpty(Origin) && !string.IsNullOrEmpty(Destination) &&
            string.Equals(Origin, Destination, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Origin and destination must be different.", new[] { nameof(Origin), nameof(Destination) });
        }

        foreach (var airlineCode in IncludedAirlineCodes.Where(airlineCode => airlineCode.Length != 2 || !Regex.IsMatch(airlineCode, "^[0-9A-Z]{2}$")))
        {
            yield return new ValidationResult($"Invalid airline code: {airlineCode}. Must be a 2-character IATA code.",
                new[] { nameof(IncludedAirlineCodes) });
        }

        foreach (var airlineCode in ExcludedAirlineCodes.Where(airlineCode => airlineCode.Length != 2 || !Regex.IsMatch(airlineCode, "^[0-9A-Z]{2}$")))
        {
            yield return new ValidationResult($"Invalid airline code: {airlineCode}. Must be a 2-character IATA code.",
                new[] { nameof(ExcludedAirlineCodes) });
        }

        if (Adults + Children > 9)
        {
            yield return new ValidationResult(
                "The total number of seated travelers (adults + children) cannot exceed 9.",
                new[] { nameof(Adults), nameof(Children) });
        }

        if (Infants > Adults)
        {
            yield return new ValidationResult(
                "The number of infants cannot exceed the number of adults.",
                new[] { nameof(Infants), nameof(Adults) });
        }

        var maxFutureDate = DateTime.Today.AddDays(365); // Most APIs limit to ~1 year
        if (DepartureDate.Date > maxFutureDate)
        {
            yield return new ValidationResult(
                "Departure date cannot be more than 365 days in the future.",
                new[] { nameof(DepartureDate) });
        }
    }
}
