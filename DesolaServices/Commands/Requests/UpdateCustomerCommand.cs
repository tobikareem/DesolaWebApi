using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Requests;

public class UpdateCustomerCommand : CustomerSignupRequest, IRequest<CustomerUpdateResponse>
{
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Email is required for identification");

        if (!string.IsNullOrEmpty(Email) && !IsValidEmail(Email))
            errors.Add("Invalid email format");

        if (string.IsNullOrEmpty(FullName) &&
            string.IsNullOrEmpty(Phone) &&
            string.IsNullOrEmpty(PreferredCurrency) &&
            string.IsNullOrEmpty(DefaultOriginAirport) &&
            (Metadata == null || !Metadata.Any()))
        {
            errors.Add("At least one field must be provided for update");
        }

        return !errors.Any();

    }

    public List<string> GetUpdatedFields()
    {
        var fields = new List<string>();

        if (!string.IsNullOrEmpty(FullName)) fields.Add("FullName");
        if (!string.IsNullOrEmpty(Phone)) fields.Add("Phone");
        if (!string.IsNullOrEmpty(PreferredCurrency)) fields.Add("PreferredCurrency");
        if (!string.IsNullOrEmpty(DefaultOriginAirport)) fields.Add("DefaultOriginAirport");
        if (Metadata?.Any() == true) fields.Add("Metadata");

        return fields;
    }
}