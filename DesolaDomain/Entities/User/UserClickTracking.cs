using Azure;
using Azure.Data.Tables;

namespace DesolaDomain.Entities.User;

public class UserClickTracking : ITableEntity
{
    private string _userId;
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
    public string PartitionKey { get; set; }
    public string RowKey { get; set; } = "CLICK_TRACKING";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}