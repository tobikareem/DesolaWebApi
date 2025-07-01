using DesolaDomain.Entities.User;
using DesolaServices.DataTransferObjects.Requests;

namespace DesolaServices.DataTransferObjects.Responses;

public class CustomerUpdateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CustomerDto Customer { get; set; }
    public List<string> UpdatedFields { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool StripeUpdated { get; set; }

    public static CustomerUpdateResponse SuccessResult(Customer customer, List<string> updatedFields, bool stripeUpdated = false)
    {
        return new CustomerUpdateResponse
        {
            Success = true,
            Message = "Customer updated successfully",
            Customer = CustomerDto.FromCustomer(customer),
            UpdatedFields = updatedFields,
            StripeUpdated = stripeUpdated
        };
    }

    public static CustomerUpdateResponse FailureResult(string message, List<string> errors = null)
    {
        return new CustomerUpdateResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static CustomerUpdateResponse ValidationFailureResult(List<string> validationErrors)
    {
        return new CustomerUpdateResponse
        {
            Success = false,
            Message = "Validation failed",
            Errors = validationErrors
        };
    }

    public static CustomerUpdateResponse NotFoundResult(string email)
    {
        return new CustomerUpdateResponse
        {
            Success = false,
            Message = $"Customer with email '{email}' not found",
            Errors = new List<string> { "Customer not found" }
        };
    }

}