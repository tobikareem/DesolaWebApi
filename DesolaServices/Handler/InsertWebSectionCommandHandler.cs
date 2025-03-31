using DesolaDomain.Entities.Pages;
using DesolaServices.Commands.Requests;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler;

public class InsertWebSectionCommandHandler : IRequestHandler<InsertWebSectionCommand>
{
    private readonly ITableBase<WebSection> _table;
    public InsertWebSectionCommandHandler(ITableBase<WebSection> table)
    {
        _table = table;
    }

    public async Task Handle(InsertWebSectionCommand request, CancellationToken cancellationToken)
    {
        await _table.InsertTableEntityAsync(request.WebSection);
    }
}
