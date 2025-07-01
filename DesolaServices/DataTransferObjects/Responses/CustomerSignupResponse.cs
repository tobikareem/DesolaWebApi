using DesolaDomain.Entities.User;
using DesolaServices.DataTransferObjects.Requests;

namespace DesolaServices.DataTransferObjects.Responses;

public class CustomerSignupResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CustomerDto Customer { get; set; }
    public string StripeCustomerId { get; set; }
    public List<string> Errors { get; set; } = new();

    public static CustomerSignupResponse SuccessResult(Customer customer, string stripeCustomerId)
    {
        return new CustomerSignupResponse
        {
            Success = true,
            Message = "Customer created successfully",
            Customer = CustomerDto.FromCustomer(customer),
            StripeCustomerId = stripeCustomerId
        };
    }

    public static CustomerSignupResponse FailureResult(string message, List<string> errors = null)
    {
        return new CustomerSignupResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static CustomerSignupResponse ValidationFailureResult(List<string> validationErrors)
    {
        return new CustomerSignupResponse
        {
            Success = false,
            Message = "Validation failed",
            Errors = validationErrors
        };
    }
}