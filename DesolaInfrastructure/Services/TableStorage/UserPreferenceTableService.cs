using Azure;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaInfrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.Services.TableStorage;

public class UserPreferenceTableService : BaseTableStorage<UserTravelPreference>
{
    private readonly IAirportRepository _airportRepository;

    public UserPreferenceTableService(
            ITableStorageRepository<UserTravelPreference> storageRepository,
            ICacheService cacheService, 
            ILogger<UserPreferenceTableService> logger,
            IOptions<AppSettings> configuration, 
            IAirportRepository airportRepository, 
            TimeSpan? cacheExpiration = null
        )
        : base(storageRepository, cacheService, logger, configuration.Value.Database.UserTravelPreferenceTableName, cacheExpiration)
    {
        _airportRepository = airportRepository;
    }

    public override async Task InsertTableEntityAsync(UserTravelPreference entity)
    {
        if (string.IsNullOrWhiteSpace(entity.UserId))
            throw new ArgumentNullException(nameof(entity.UserId), "UserId is required for UserTravelPreference.");

        await ValidateAirportCodesAsync(entity.OriginAirport, entity.DestinationAirport);

        await base.InsertTableEntityAsync(entity);
    }

    protected override string GetPartitionKey(UserTravelPreference entity) => entity.PartitionKey;
    protected override string GetRowKey(UserTravelPreference entity) => entity.RowKey;
    protected override ETag GetETag(UserTravelPreference entity) => entity.ETag;
    protected override void SetETag(UserTravelPreference entity, Azure.ETag etag) => entity.ETag = etag;

    private async Task ValidateAirportCodesAsync(string originAirportCode, string destinationAirportCode)
    {
        if (string.IsNullOrWhiteSpace(originAirportCode) || originAirportCode.Length < 3)
            throw new ArgumentNullException(nameof(originAirportCode), "Origin Airport code is required.");

        if (string.IsNullOrWhiteSpace(destinationAirportCode) || destinationAirportCode.Length < 3)
            throw new ArgumentNullException(nameof(destinationAirportCode), "Destination Airport code is required.");

        var originAirport = await _airportRepository.IsAirportValidAsync(originAirportCode);
        var destinationAirport = await _airportRepository.IsAirportValidAsync(destinationAirportCode);

        if (!originAirport || !destinationAirport)
            throw new ArgumentException("Invalid airport code.");

    }
}