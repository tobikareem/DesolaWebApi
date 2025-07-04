using AutoMapper;
using DesolaServices.Commands.Requests;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Payments;

public class NewUserSignUpCommandHandler: IRequestHandler<NewUserSignUpCommand, CustomerSignupResponse>
{
    private readonly ICustomerManagementService _customerManagementService;
    private readonly ILogger<NewUserSignUpCommandHandler> _logger;
    private readonly IMapper _mapper;

    public NewUserSignUpCommandHandler(ICustomerManagementService customerManagementService, ILogger<NewUserSignUpCommandHandler> logger, IMapper mapper)
    {
        _customerManagementService = customerManagementService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<CustomerSignupResponse> Handle(NewUserSignUpCommand request, CancellationToken cancellationToken)
    {
        
        try
        {
            _logger.LogInformation("Processing user signup for email: {Email}", request.Email);
            
            if (!request.IsValid(out var validationErrors))
            {
                _logger.LogWarning("Validation failed for signup request: {Email}. Errors: {Errors}",
                    request.Email, string.Join(", ", validationErrors));
                return CustomerSignupResponse.ValidationFailureResult(validationErrors);
            }
            
            var signupRequest = _mapper.Map<CustomerSignupRequest>(request);
            
            var result = await _customerManagementService.CreateCustomerAsync(signupRequest, cancellationToken);
            
            if (result.Success)
            {
                _logger.LogInformation("User signup successful for email: {Email}, StripeId: {StripeId}",
                    request.Email, result.StripeCustomerId);

                return CustomerSignupResponse.SuccessResult(result.Customer, result.StripeCustomerId);
            }

            _logger.LogWarning("User signup failed for email: {Email}. Reason: {Reason}",
                request.Email, result.ErrorMessage);

            return CustomerSignupResponse.FailureResult(result.ErrorMessage);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument during user signup: {Email}", request.Email);
            return CustomerSignupResponse.FailureResult("Invalid input provided");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation during user signup: {Email}", request.Email);
            return CustomerSignupResponse.FailureResult("Unable to process signup at this time");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user signup: {Email}", request.Email);
            return CustomerSignupResponse.FailureResult("An unexpected error occurred during signup");
        }
    }

}