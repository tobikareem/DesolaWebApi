using DesolaDomain.Entities.User;
using DesolaServices.Commands.Queries;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler;

public class GetUserTravelPreferenceQueryHandler: IRequestHandler<GetUserTravelPreferenceQuery, UserTravelPreference>
{
    private readonly ITableBase<UserTravelPreference> _table;
    public GetUserTravelPreferenceQueryHandler(ITableBase<UserTravelPreference> table)
    {
        _table = table;
    }
    public async Task<UserTravelPreference> Handle(GetUserTravelPreferenceQuery request, CancellationToken cancellationToken)
    {
        return await _table.GetTableEntityAsync(request.UserId, new UserTravelPreference().RowKey);
    }
}