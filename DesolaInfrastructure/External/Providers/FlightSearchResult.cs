using AutoMapper;
using DesolaDomain.Entities.FlightSearch;

namespace DesolaInfrastructure.External.Providers;

public class FlightSearchResult<T>
{
    public T RawResponse { get; set; }
    public UnifiedFlightSearchResponse UnifiedResponse { get; set; }

    public static async Task<FlightSearchResult<T>> GetFromMappedApiAsync(T rawResponse, IMapper mapper, object mappingContext, CancellationToken cancellationToken)
    {
        var unifiedResponse = mapper.Map<UnifiedFlightSearchResponse>(rawResponse, opt =>
        {
            if (mappingContext == null) return;

            foreach (var property in mappingContext.GetType().GetProperties())
            {
                opt.Items[property.Name] = property.GetValue(mappingContext);
            }
        });

        var response = new FlightSearchResult<T>
        {
            RawResponse = rawResponse,
            UnifiedResponse = unifiedResponse
        };

        return await Task.FromResult(response);
    }
}