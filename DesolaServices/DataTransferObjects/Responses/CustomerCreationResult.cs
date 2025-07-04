using DesolaDomain.Entities.User;

namespace DesolaServices.DataTransferObjects.Responses;

public class CustomerCreationResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public Customer Customer { get; set; }
    public string StripeCustomerId { get; set; }
}