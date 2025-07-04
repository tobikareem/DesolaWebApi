namespace DesolaServices.DataTransferObjects.Responses;

public class ClickHistoryResponse
{
    public List<ClickHistoryItem> Results { get; set; }
    public int PageSize { get; set; }
    public int TotalResults { get; set; }
    public bool HasMoreResults { get; set; }
    public string NextPageToken { get; set; }
}