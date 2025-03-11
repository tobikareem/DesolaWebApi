using DesolaDomain.Settings;
using DesolaInfrastructure;
using DesolaServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.Configure<AppSettings>(configuration);

        var appSettings = new AppSettings();
        configuration.Bind(appSettings);

        services.AddSingleton(appSettings);

        services.AddMemoryCache();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDesolaInfrastructure(appSettings);
        services.AddDesolaApplications(appSettings);
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("https://desolatravels.com",
                        "https://www.desolatravels.com",
                        "http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {

            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

            options.TokenValidationParameters.ValidIssuers = new[]
            {
                $"{appSettings.AzureB2C.Instance}{appSettings.AzureB2C.TenantId}/v2.0/",
                $"{appSettings.AzureB2C.Instance}{appSettings.AzureB2C.Domain}/v2.0/"
            };

            // Configure valid audiences
            options.TokenValidationParameters.ValidAudiences = new[]
            {
                appSettings.AzureB2C.ClientId,
                $"api://{appSettings.AzureB2C.ClientId}",
                appSettings.AzureB2C.ApplicationIdUri
            };
            
            options.Authority = $"{appSettings.AzureB2C.Instance}{appSettings.AzureB2C.TenantId}/v2.0/.well-known/openid-configuration?p={appSettings.AzureB2C.SignUpSignInPolicy}";
            options.MetadataAddress = $"{appSettings.AzureB2C.Instance}{appSettings.AzureB2C.TenantId}/v2.0/.well-known/openid-configuration?p={appSettings.AzureB2C.SignUpSignInPolicy}";

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = a =>
                {
                    logger.LogError($"Authentication failed: {a.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnMessageReceived = a =>
                {
                    logger.LogInformation($"Token received: {a.Token?.Substring(0, 10)}...");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureB2C"));

        services.AddAuthorization();

    })
    .Build();

host.Run();
