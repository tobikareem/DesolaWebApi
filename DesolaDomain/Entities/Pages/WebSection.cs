using Azure;
using Azure.Data.Tables;

namespace DesolaDomain.Entities.Pages;

public class WebSection: ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string RowValue { get; set; }
    public string ImagePath { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string AdditionalNotes { get; set; }

}