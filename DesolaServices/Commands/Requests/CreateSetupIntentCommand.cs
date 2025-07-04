using CaptainPayment.Core.Models;
using MediatR;

namespace DesolaServices.Commands.Requests;

public class CreateSetupIntentCommand: IRequest<SetupIntentResult>
{
    public string UserId { get; set; }

    public CreateSetupIntentRequest CreateSetupIntentRequest { get; set; }

    public CreateSetupIntentCommand(string userId, CreateSetupIntentRequest createSetupIntentRequest)
    {
        UserId = userId;
        CreateSetupIntentRequest = createSetupIntentRequest;
    }
}