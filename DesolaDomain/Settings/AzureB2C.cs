namespace DesolaDomain.Settings;

public class AzureB2C
{
    public string Authority { get; set; }
    public string Instance { get; set; }
    public string Domain { get; set; }
    public string SignUpSignInPolicy { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
    public string ApplicationIdUri { get; set; }
    public string ApplicationScope { get; set; }
    public string CodeVerifier { get; set; }
    public string CallbackPath { get; set; }
    public string TenantId { get; set; }
}