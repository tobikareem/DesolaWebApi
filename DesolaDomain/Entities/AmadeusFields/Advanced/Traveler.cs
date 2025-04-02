namespace DesolaDomain.Entities.AmadeusFields.Advanced;

public class Traveler
{
    public string Id { get; set; }
    public string TravelerType { get; set; } // ADULT, CHILD, INFANT, etc.
    public string AssociatedAdultId { get; set; } // For infants
}