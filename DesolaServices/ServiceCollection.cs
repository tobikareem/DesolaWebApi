using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DesolaServices.Interfaces;
using DesolaServices.Services;

namespace DesolaServices
{
    public static class ServiceCollection
    {
        public static IServiceCollection AddDesolaApplications(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddScoped<IFlightSearchService, FlightSearchService>();

            services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
