namespace Desola.Common.Exceptions;

public class CustomerNotFoundException : Exception
{
    public string CustomerIdentifier { get; }

    public CustomerNotFoundException(string customerIdentifier)
        : base($"Customer not found: {customerIdentifier}")
    {
        CustomerIdentifier = customerIdentifier;
    }

    public CustomerNotFoundException(string customerIdentifier, string message)
        : base(message)
    {
        CustomerIdentifier = customerIdentifier;
    }

    public CustomerNotFoundException(string customerIdentifier, string message, Exception innerException)
        : base(message, innerException)
    {
        CustomerIdentifier = customerIdentifier;
    }
}