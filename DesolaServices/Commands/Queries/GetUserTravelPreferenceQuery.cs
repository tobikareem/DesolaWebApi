using DesolaDomain.Entities.User;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class GetUserTravelPreferenceQuery: IRequest<UserTravelPreference>
{
    public string UserId { get; }

    public GetUserTravelPreferenceQuery(string userId)
    {
        UserId = userId;
    }
}