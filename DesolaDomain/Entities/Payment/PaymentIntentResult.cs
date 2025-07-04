using Azure;
using Azure.Data.Tables;
using CaptainPayment.Core.Models;

namespace DesolaDomain.Entities.Payment;

public class PaymentIntentResult: SetupIntentResult, ITableEntity
{
    public string UserId { get; set; }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}