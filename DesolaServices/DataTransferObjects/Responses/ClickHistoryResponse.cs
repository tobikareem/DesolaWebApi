namespace DesolaServices.DataTransferObjects.Responses;

public class ClickHistoryResponse
{
    public List<ClickHistoryItem> Results { get; set; }
    public int PageSize { get; set; }
    public int TotalResults { get; set; }
    public bool HasMoreResults { get; set; }
    public string NextPageToken { get; set; }
}

public class ClickHistoryItem
{
    public string Id { get; set; }
    public string ClickedAt { get; set; }
    public string FlightOffer { get; set; }
    public string FlightOrigin { get; set; }
    public string FlightDestination { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; }
    public string ErrorDescription { get; set; }
}