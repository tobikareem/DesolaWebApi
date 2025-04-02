namespace DesolaDomain.Settings;

public class AmadeusApi
{
    public string BaseUrl { get; set; }
    public string FlightSearchUrl { get; set; }
    public string TokenEndpointUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public string ProviderName { get; set; }
}