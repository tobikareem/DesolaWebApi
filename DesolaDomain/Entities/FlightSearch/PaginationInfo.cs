namespace DesolaDomain.Entities.FlightSearch;

public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string NextPageToken { get; set; }
}