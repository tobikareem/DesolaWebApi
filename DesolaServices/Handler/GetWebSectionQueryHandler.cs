using DesolaDomain.Entities.PageEntity;
using DesolaServices.Commands.Queries;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler;

public class GetWebSectionQueryHandler : IRequestHandler<GetWebSectionQuery, WebSection>
{
    private readonly ITableBase<WebSection> _table;

    public GetWebSectionQueryHandler(ITableBase<WebSection> table)
    {
        _table  = table;
    }

    public async Task<WebSection> Handle(GetWebSectionQuery request, CancellationToken cancellationToken)
    {
        return await _table.GetTableEntityAsync(request.PartitionKey, request.RowKey);
    }
}