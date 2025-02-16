using DesolaInfrastructure;
using DesolaServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddMemoryCache();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDesolaInfrastructure(configuration);
        services.AddDesolaApplications(configuration);
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("*")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

    })
    .Build();

host.Run();
