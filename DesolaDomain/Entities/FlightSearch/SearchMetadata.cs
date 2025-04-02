using DesolaDomain.Entities.AmadeusFields.Basic;

namespace DesolaDomain.Entities.FlightSearch;

public class SearchMetadata
{
    public Guid SearchId { get; set; }
    public DateTime SearchTimestamp { get; set; }
    public FlightSearchParameters Parameters { get; set; }
    public List<string> ActiveFilters { get; set; }
    public PaginationInfo Pagination { get; set; }
}