using Azure;
using Azure.Data.Tables;

namespace DesolaDomain.Entities.User;
public class UserTravelPreference : ITableEntity
{
    private string _userId;

    public string OriginAirport { get; set; }
    public string DestinationAirport { get; set; }
    public string TravelClass { get; set; }
    public string StopOvers { get; set; }
    public string UserId
    {
        get => _userId;
        set
        {
            _userId = value;
            PartitionKey = value;
        }
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; } = "PREFERENCE";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}