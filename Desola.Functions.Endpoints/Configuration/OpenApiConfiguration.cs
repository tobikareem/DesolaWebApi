using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace Desola.Functions.Endpoints.Configuration;

public class OpenApiConfiguration : DefaultOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new()
    {
        Version = "v1.0.0",
        Title = "Desola Flights API",
        Description = "A comprehensive flight search and booking platform API that finds the cheapest and most flexible flight options.",
        Contact = new OpenApiContact
        {
            Name = "Desola Flights Support",
            Email = "support@desolaflights.com",
            Url = new Uri("https://desolaflights.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;

    public override bool IncludeRequestingHostName { get; set; } = true;

    public override bool ForceHttps { get; set; } = !IsLocalDevelopment();

    private static bool IsLocalDevelopment()
    {
        var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
        return string.IsNullOrEmpty(environment) || environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
    }

}