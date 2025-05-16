using Azure;
using Azure.Data.Tables;

namespace DesolaDomain.Entities.User;

public class UserClickTracking : ITableEntity
{
    private string _userId;

    public UserClickTracking()
    {
        RowKey = $"CLICK_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid().ToString("N")[..6]}";
        ClickedAt = DateTime.UtcNow.ToString("o");
    }

    public string UserId
    {
        get => _userId;
        set
        {
            _userId = value;
            PartitionKey = value;
        }
    }

    public string FlightOffer { get; set; }
    public string ClickedAt { get; set; }

    public string FlightOrigin { get; set; }
    public string FlightDestination { get; set; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public ETag ETag { get; set; }

    public string UserFriendlyName { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}