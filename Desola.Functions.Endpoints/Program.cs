using Desola.Functions.Endpoints.Configuration;
using DesolaDomain.Settings;
using DesolaInfrastructure;
using DesolaServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
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

        if (appSettings.ExternalApi == null)
        {
            var valuesSection = configuration.GetSection("Values");
            services.Configure<AppSettings>(valuesSection);
            appSettings = new AppSettings();
            valuesSection.Bind(appSettings);
        }

        services.AddSingleton(appSettings);
        services.AddMemoryCache();

        if (!context.HostingEnvironment.IsDevelopment())
        {
            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();
        }

        services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
        {
            var options = new OpenApiConfiguration();
            return options;
        });

        services.AddDesolaInfrastructure(appSettings);
        services.AddDesolaApplications(appSettings);
        //services.AddCors(options =>
        //{
        //    options.AddDefaultPolicy(builder =>
        //    {
        //        builder.WithOrigins("https://desolatravels.com", "https://www.desolatravels.com", "http://localhost:5173")
        //            .AllowAnyMethod()
        //            .AllowAnyHeader();
        //    });
        //});

        services.AddCors(options =>
        {
            if (context.HostingEnvironment.IsDevelopment())
            {
                //// Development CORS - more permissive
                //options.AddDefaultPolicy(builder =>
                //{
                //    builder.AllowAnyOrigin()
                //        .AllowAnyMethod()
                //        .AllowAnyHeader();
                //});

                // Specific policy for OpenAPI endpoints
                options.AddPolicy("OpenApiPolicy", builder =>
                {
                    builder.WithOrigins(
                            "http://localhost:7094",
                            "https://localhost:7094",
                            "http://localhost:5173",
                            "https://localhost:5173")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            }
            else
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(
                            "https://desolatravels.com",
                            "https://www.desolatravels.com")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });

                options.AddPolicy("OpenApiPolicy", builder =>
                {
                    builder.WithOrigins(
                            "https://desolatravels.com",
                            "https://www.desolatravels.com",
                            "https://desolafunctionsapp.azurewebsites.net")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            }
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

    }).Build();

host.Run();
