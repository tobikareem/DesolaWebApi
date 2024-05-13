using DesolaDomain.Model;
using DesolaInfrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

#if DEBUG
        var msiEnvironment = new MsiEnvironment
        {
            ClientSecret = configuration["Azure_Client_Secret"],
            ClientId = configuration["Azure_Client_Id"],
            TenantId = configuration["Azure_Tenant_Id"]
        };

        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", msiEnvironment.ClientId);
        Environment.SetEnvironmentVariable("AZURE_TENANT_ID", msiEnvironment.TenantId);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", msiEnvironment.ClientSecret);
#endif

        services.AddMemoryCache();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDesolaInfrastructure(configuration);
    })
    .Build();

host.Run();
