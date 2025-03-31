using DesolaDomain.Entities.User;
using MediatR;

namespace DesolaServices.Commands.Requests;

public class InsertUserTravelPreferenceCommand: IRequest
{
    public UserTravelPreference UserTravelPreference { get; }
    public InsertUserTravelPreferenceCommand(UserTravelPreference userTravelPreference)
    {
        UserTravelPreference = userTravelPreference;
    }
}