using DesolaDomain.Entities.Pages;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class GetWebSectionQuery : IRequest<WebSection>
{
    public string PartitionKey { get; }
    public string RowKey { get; }

    public GetWebSectionQuery(string partitionKey, string rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }
}