namespace DesolaDomain.Settings;

public class AppSettings
{
    public string FunctionsWorkerRuntime { get; set; }
    public string AzureWebJobsStorage { get; set; }
    public StorageAccount StorageAccount { get; set; }
    public BlobFiles BlobFiles { get; set; }
    public ExternalApi ExternalApi { get; set; }
    public Airlines Airlines { get; set; }
    public Database Database { get; set; }
    public AzureB2C AzureB2C { get; set; }
}

// Storage Account 
public class StorageAccount
{
    public string AccountName { get; set; }
    public string ContainerName { get; set; }
    public string ConnectionString { get; set; }
}

// Blob File Paths
public class BlobFiles
{
    public string AirportFile { get; set; }
    public string AirportCodeFile { get; set; }
    public string SkyScannerAirportFile { get; set; }
    public string BlobUri { get; set; }
    public string AirportCodeBlobUri { get; set; }
}

// External API 
public class ExternalApi
{
    public AmadeusApi Amadeus { get; set; }
    public RapidApi RapidApi { get; set; }
}

public class AmadeusApi
{
    public string BaseUrl { get; set; }
    public string TokenEndpointUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}

public class RapidApi
{
    public string SkyScannerHost { get; set; }
    public string SkyScannerKey { get; set; }
    public string SkyScannerUri { get; set; }
}

// Airlines Information
public class Airlines
{
    public string UnitedStatesAirlines { get; set; }
}

// Database 
public class Database
{
    public string WebPageContentTableName { get; set; }
    public string UserTravelPreferenceTableName { get; set; }
}

// Azure B2C Authentication
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
