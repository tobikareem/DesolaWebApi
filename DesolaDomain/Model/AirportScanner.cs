
namespace DesolaDomain.Model;

public class AirportScanner
{
    public List<AutocompleteData> Data { get; set; }
    public bool Status { get; set; }
    public string Message { get; set; }
}

public class AutocompleteData
{
    public AutocompletePresentation Presentation { get; set; }
    public AutocompleteNavigation Navigation { get; set; }
}

public class AutocompletePresentation
{
    public string Title { get; set; }
    public string SuggestionTitle { get; set; }
    public string Subtitle { get; set; }
    public string Id { get; set; }
    public string SkyId { get; set; }
}

public class AutocompleteNavigation
{
    public string EntityId { get; set; }
    public string EntityType { get; set; }
    public string LocalizedName { get; set; }
}