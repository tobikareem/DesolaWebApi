using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaServices.Commands.Requests;
using MediatR;

namespace DesolaServices.Handler;

public class InsertUserTravelPreferenceCommandHandler : IRequest<InsertUserTravelPreferenceCommand>
{
    private readonly ITableBase<UserTravelPreference> _table;

    public InsertUserTravelPreferenceCommandHandler(ITableBase<UserTravelPreference> table)
    {
        _table = table;
    }

    public async Task Handle(InsertUserTravelPreferenceCommand request, CancellationToken cancellationToken)
    {
        var existing = await _table.GetTableEntityAsync(request.UserTravelPreference.PartitionKey, request.UserTravelPreference.RowKey);
        if (!string.IsNullOrWhiteSpace(existing.UserId))
        {
            await _table.UpdateTableEntityAsync(request.UserTravelPreference);
        }
        else
        {
            await _table.InsertTableEntityAsync(request.UserTravelPreference);
        }
    }
}