using DesolaDomain.Entities.PageEntity;
using DesolaServices.Commands.Requests;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler;

public class UpdateWebSectionCommandHandler : IRequestHandler<UpdateWebSectionCommand>
{
    private readonly ITableBase<WebSection> _table;
    public UpdateWebSectionCommandHandler(ITableBase<WebSection> table)
    {
        _table = table;
    }

    public async Task Handle(UpdateWebSectionCommand request, CancellationToken cancellationToken)
    {
        await _table.UpdateTableEntityAsync(request.WebSection);
    }
}
