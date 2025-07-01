using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Requests;

public class NewUserSignUpCommand: CustomerSignupRequest, IRequest<CustomerSignupResponse>
{
    /// <summary>
    /// Validates the command
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Email is required");

        if (string.IsNullOrWhiteSpace(FullName))
            errors.Add("Full name is required");

        if (!string.IsNullOrEmpty(Email) && !IsValidEmail(Email))
            errors.Add("Invalid email format");

        return !errors.Any();
    }

}