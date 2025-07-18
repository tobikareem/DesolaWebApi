using CaptainPayment.Core.Models;
using DesolaDomain.Entities.Payment;
using DesolaServices.Commands.Queries.Payment;
using DesolaServices.Commands.Requests;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;

namespace Desola.Functions.Endpoints.Functions;

/// <summary>
/// Payment and Subscription Management API endpoints for Desola Flights platform
/// </summary>
public class Payment
{
    private readonly ILogger<Payment> _logger;
    private readonly IMediator _mediator;
    private readonly IDesolaSubscriptionService _desolaSubscriptionService;

    public Payment(ILogger<Payment> logger, IMediator mediator, IDesolaSubscriptionService desolaProductAndPriceStorage)
    {
        _logger = logger;
        _mediator = mediator;
        _desolaSubscriptionService = desolaProductAndPriceStorage;
    }

    #region Customer Management

    [Function("CustomerSignup")]
    [OpenApiOperation(
        operationId: "CustomerSignup",
        tags: new[] { "Customer Management" },
        Summary = "Register a new customer",
        Description = "Creates a new customer account in the Desola Flights platform. This endpoint handles customer registration with email validation and creates associated Stripe customer records.",
        Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = (OpenApiSecurityLocationType)ParameterLocation.Header)]
    [OpenApiRequestBody(
        contentType: "application/json",
        bodyType: typeof(NewUserSignUpCommand),
        Required = true,
        Description = "Customer registration details including email, full name, phone, and preferences",
        Example = typeof(CustomerSignupRequest))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(CustomerSignupResponse),
        Summary = "Customer registration successful",
        Description = "Customer has been successfully registered and Stripe customer account created",
        Example = typeof(CustomerSignupResponse))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(CustomerSignupResponse),
        Summary = "Registration failed",
        Description = "Invalid data provided or customer already exists with the same email")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.Unauthorized,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Authentication required",
        Description = "Valid authentication token is required to access this endpoint")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.InternalServerError,
        contentType: "application/json",
        bodyType: typeof(CustomerSignupResponse),
        Summary = "Server error",
        Description = "An unexpected error occurred during customer registration")]
    public async Task<IActionResult> CustomerSignup(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customer/signup")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Customer signup request triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var command = JsonConvert.DeserializeObject<NewUserSignUpCommand>(requestBody);

            if (command == null)
            {
                return new BadRequestObjectResult(CustomerSignupResponse.FailureResult("Invalid or missing signup data."));
            }

            var isAuthenticated = await IsUserAuthenticated(req);

            if (!isAuthenticated.authenticationStatus)
            {
                return isAuthenticated.authenticationResponse!;
            }

            req.HttpContext.VerifyUserHasAnyAcceptedScope("Files.Read");

            command.CustomerId = req.HttpContext.User.Identity is { IsAuthenticated: true } ? req.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

            var result = await _mediator.Send(command, cancellationToken);

            return result.Success
                ? new OkObjectResult(result)
                : new BadRequestObjectResult(result);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in customer signup request");
            return new BadRequestObjectResult(CustomerSignupResponse.FailureResult("Invalid JSON format"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer signup request");
            return new ObjectResult(CustomerSignupResponse.FailureResult("Internal server error"))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("UpdateCustomer")]
    [OpenApiOperation(
        operationId: "UpdateCustomer",
        tags: new[] { "Customer Management" },
        Summary = "Update customer information",
        Description = "Updates comprehensive customer profile information including personal details, preferences, and contact information. Supports partial updates.",
        Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = (OpenApiSecurityLocationType)ParameterLocation.Header)]
    [OpenApiRequestBody(
        contentType: "application/json",
        bodyType: typeof(UpdateCustomerCommand),
        Required = true,
        Description = "Customer update information. Only provided fields will be updated.",
        Example = typeof(UpdateCustomerCommand))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(CustomerUpdateResponse),
        Summary = "Customer updated successfully",
        Description = "Customer information has been successfully updated")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(CustomerUpdateResponse),
        Summary = "Invalid update data",
        Description = "Validation failed due to invalid or missing required fields")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.NotFound,
        contentType: "application/json",
        bodyType: typeof(CustomerUpdateResponse),
        Summary = "Customer not found",
        Description = "No customer exists with the provided identifier")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.Unauthorized,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Authentication required",
        Description = "Valid authentication token is required")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.InternalServerError,
        contentType: "application/json",
        bodyType: typeof(CustomerUpdateResponse),
        Summary = "Server error",
        Description = "An unexpected error occurred during update")]
    public async Task<IActionResult> UpdateCustomer(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "customer/update")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Customer update request triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var command = JsonConvert.DeserializeObject<UpdateCustomerCommand>(requestBody);

            if (command == null)
            {
                return new BadRequestObjectResult(CustomerUpdateResponse.FailureResult("Invalid or missing update data."));
            }

            var isAuthenticated = await IsUserAuthenticated(req);

            if (!isAuthenticated.authenticationStatus)
            {
                return isAuthenticated.authenticationResponse!;
            }

            req.HttpContext.VerifyUserHasAnyAcceptedScope("Files.Read");

            command.CustomerId = req.HttpContext.User.Identity is { IsAuthenticated: true } ? req.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

            var result = await _mediator.Send(command, cancellationToken);

            return result.Success switch
            {
                true => new OkObjectResult(result),
                false when result.Message.Contains("not found") => new NotFoundObjectResult(result),
                false => new BadRequestObjectResult(result)
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in customer update request");
            return new BadRequestObjectResult(CustomerUpdateResponse.FailureResult("Invalid JSON format"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer update request");
            return new ObjectResult(CustomerUpdateResponse.FailureResult("Internal server error"))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("GetCustomer")]
    [OpenApiOperation(
        operationId: "GetCustomer",
        tags: new[] { "Customer Management" },
        Summary = "Retrieve customer by email",
        Description = "Fetches customer profile information using email address. Returns comprehensive customer data including subscription status and preferences.",
        Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(
        name: "email",
        In = ParameterLocation.Query,
        Required = true,
        Type = typeof(string),
        Description = "Customer's email address (case-insensitive)"
        )]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(CustomerDto),
        Summary = "Customer found",
        Description = "Customer profile data retrieved successfully",
        Example = typeof(CustomerDto))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.NotFound,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Customer not found",
        Description = "No customer exists with the provided email address")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Invalid email",
        Description = "Email parameter is missing or has invalid format")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.InternalServerError,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Server error",
        Description = "An unexpected error occurred")]
    [OpenApiSecurity("x-functions-key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = (OpenApiSecurityLocationType)ParameterLocation.Header)]

    public async Task<IActionResult> GetCustomer(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customer")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get customer request triggered.");

        try
        {
            var email = req.Query["email"].FirstOrDefault();

            if (string.IsNullOrEmpty(email))
            {
                return new BadRequestObjectResult(new { error = "Email parameter is required" });
            }

            var query = new GetCustomerByEmailQuery(email);
            var result = await _mediator.Send(query, cancellationToken);

            return result != null
                ? new OkObjectResult(result)
                : new NotFoundObjectResult(new { error = $"Customer with email '{email}' not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get customer request");
            return new ObjectResult(new { error = "Internal server error" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("UpdateCustomerField")]
    [OpenApiOperation(
        operationId: "UpdateCustomerField",
        tags: new[] { "Customer Management" },
        Summary = "Update specific customer field",
        Description = "Updates a single customer profile field. Useful for targeted updates without sending the entire customer object. Supports atomic field updates.",
        Visibility = OpenApiVisibilityType.Advanced)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = (OpenApiSecurityLocationType)ParameterLocation.Header)]
    [OpenApiParameter(
        name: "email",
        In = ParameterLocation.Path,
        Required = true,
        Type = typeof(string),
        Description = "Customer's email address"
        )]
    [OpenApiParameter(
        name: "field",
        In = ParameterLocation.Path,
        Required = true,
        Type = typeof(string),
        Description = "Field name to update. Supported values: 'fullname', 'name', 'phone', 'currency', 'airport'"
        )]
    [OpenApiRequestBody(
        contentType: "application/json",
        bodyType: typeof(string),
        Required = true,
        Description = "New field value as JSON string"
        )]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(CustomerUpdateResponse),
        Summary = "Field updated successfully",
        Description = "The specified field has been updated")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(CustomerUpdateResponse),
        Summary = "Invalid field or value",
        Description = "The field name is not supported or the value is invalid")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.NotFound,
        contentType: "application/json",
        bodyType: typeof(CustomerUpdateResponse),
        Summary = "Customer not found",
        Description = "No customer exists with the provided email")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.Unauthorized,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Authentication required",
        Description = "Valid authentication token is required")]
    public async Task<IActionResult> UpdateCustomerField(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "customer/{email}/field/{field}")] HttpRequest req,
        string email,
        string field,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update customer field request triggered for email: {Email}, field: {Field}", email, field);

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var value = JsonConvert.DeserializeObject<string>(requestBody);

            if (string.IsNullOrEmpty(value))
            {
                return new BadRequestObjectResult(CustomerUpdateResponse.FailureResult("Field value is required"));
            }

            var command = new UpdateCustomerCommand
            {
                Email = email,
            };

            var isAuthenticated = await IsUserAuthenticated(req);

            if (!isAuthenticated.authenticationStatus)
            {
                return isAuthenticated.authenticationResponse!;
            }

            req.HttpContext.VerifyUserHasAnyAcceptedScope("Files.Read");

            command.CustomerId = req.HttpContext.User.Identity is { IsAuthenticated: true } ? req.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

            // Set the specific field based on the parameter
            switch (field.ToLowerInvariant())
            {
                case "fullname":
                case "name":
                    command.FullName = value;
                    break;
                case "phone":
                    command.Phone = value;
                    break;
                case "currency":
                    command.PreferredCurrency = value;
                    break;
                case "airport":
                    command.DefaultOriginAirport = value;
                    break;
                default:
                    return new BadRequestObjectResult(CustomerUpdateResponse.FailureResult($"Field '{field}' is not updatable"));
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Success switch
            {
                true => new OkObjectResult(result),
                false when result.Message.Contains("not found") => new NotFoundObjectResult(result),
                false => new BadRequestObjectResult(result)
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in update field request");
            return new BadRequestObjectResult(CustomerUpdateResponse.FailureResult("Invalid JSON format"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update customer field request");
            return new ObjectResult(CustomerUpdateResponse.FailureResult("Internal server error"))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("GetCustomerSubscription")]
    [OpenApiOperation(
        operationId: "GetCustomerSubscription",
        tags: new[] { "Customer Management", "Subscription Management" },
        Summary = "Get customer subscription details",
        Description = "Retrieves comprehensive subscription information for a customer using multiple identifier options. Returns subscription status, billing details, and plan information.",
        Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(
        name: "email",
        In = ParameterLocation.Query,
        Required = false,
        Type = typeof(string),
        Description = "Customer's email address"
        )]
    [OpenApiParameter(
        name: "stripeCustomerId",
        In = ParameterLocation.Query,
        Required = false,
        Type = typeof(string),
        Description = "Stripe customer identifier (starts with 'cus_')"
        )]
    [OpenApiParameter(
        name: "customerId",
        In = ParameterLocation.Query,
        Required = false,
        Type = typeof(string),
        Description = "Internal customer identifier (GUID)"
        )]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(CustomerSubscriptionResponse),
        Summary = "Subscription information retrieved",
        Description = "Customer subscription details including status, plan, and billing information",
        Example = typeof(CustomerSubscriptionResponse))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.NotFound,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Customer not found",
        Description = "No customer found with any of the provided identifiers")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Missing identifiers",
        Description = "At least one identifier (email, stripeCustomerId, or customerId) must be provided")]
    [OpenApiSecurity("x-functions-key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = (OpenApiSecurityLocationType)ParameterLocation.Header)]
    public async Task<IActionResult> GetCustomerSubscription(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customer/subscription")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Customer subscription information request triggered.");

        try
        {
            var email = req.Query["email"];
            var stripeCustomerId = req.Query["stripeCustomerId"];
            var customerId = req.Query["customerId"];

            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(stripeCustomerId) && string.IsNullOrEmpty(customerId))
            {
                return new BadRequestObjectResult(new
                {
                    Error = "At least one identifier must be provided: email, stripeCustomerId, or customerId"
                });
            }

            var request = new GetCustomerSubscriptionRequest
            {
                Email = email,
                StripeCustomerId = stripeCustomerId,
                CustomerId = customerId
            };

            var result = await _mediator.Send(new GetCustomerSubscriptionQuery(request), cancellationToken);

            if (result == null)
            {
                return new NotFoundObjectResult(new
                {
                    Error = "Customer not found",
                    SearchCriteria = new { email, stripeCustomerId, customerId }
                });
            }

            return new OkObjectResult(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer subscription information");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    #endregion

    #region Subscription Management

    [Function("CreateSubscriptionDirect")]
    [OpenApiOperation(
        operationId: "CreateSubscriptionDirect",
        tags: new[] { "Subscription Management" },
        Summary = "Create subscription with payment method",
        Description = "Creates a new subscription directly with payment method in a single step. Handles customer creation, payment method attachment, and subscription activation with optional trial period.",
        Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = (OpenApiSecurityLocationType)ParameterLocation.Header)]
    [OpenApiRequestBody(
        contentType: "application/json",
        bodyType: typeof(CreateDirectSubscriptionCommand),
        Required = true,
        Description = "Subscription creation request with customer details, payment method, and plan selection",
        Example = typeof(CreateDirectSubscriptionCommand))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(CreateSubscriptionResult),
        Summary = "Subscription created successfully",
        Description = "Subscription has been created and activated. May require additional authentication.",
        Example = typeof(CreateSubscriptionResult))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Invalid subscription data",
        Description = "Required fields are missing or payment method is invalid")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.PaymentRequired,
        contentType: "application/json",
        bodyType: typeof(CreateSubscriptionResult),
        Summary = "Payment authentication required",
        Description = "Subscription created but requires additional payment authentication (3D Secure)")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.Unauthorized,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Authentication required",
        Description = "Valid authentication token is required")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.InternalServerError,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Server error",
        Description = "Payment processing failed or internal error occurred")]
    public async Task<IActionResult> CreateSubscriptionDirect(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "subscription/create-direct")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Direct subscription creation request triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var command = JsonConvert.DeserializeObject<CreateDirectSubscriptionCommand>(requestBody);

            if (command == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid subscription data" });
            }

            var isAuthenticated = await IsUserAuthenticated(req);

            if (!isAuthenticated.authenticationStatus)
            {
                return isAuthenticated.authenticationResponse!;
            }

            req.HttpContext.VerifyUserHasAnyAcceptedScope("Files.Read");

            command.UserId = req.HttpContext.User.Identity is { IsAuthenticated: true } ? req.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

            var result = await _mediator.Send(command, cancellationToken);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating direct subscription");
            return new ObjectResult(new { error = ex.Message })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("CancelSubscription")]
    [OpenApiOperation(
        operationId: "CancelSubscription",
        tags: new[] { "Subscription Management" },
        Summary = "Cancel customer subscription",
        Description = "Cancels an active subscription with options for immediate or end-of-period cancellation. Supports cancellation reasons for analytics and customer feedback.",
        Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody(
        contentType: "application/json",
        bodyType: typeof(CancelSubscriptionCommand),
        Required = true,
        Description = "Subscription cancellation details including customer identifier and cancellation preferences",
        Example = typeof(CancelSubscriptionCommand))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(CancelSubscriptionResponse),
        Summary = "Subscription cancelled successfully",
        Description = "Subscription has been cancelled according to the specified preferences",
        Example = typeof(CancelSubscriptionResponse))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(CancelSubscriptionResponse),
        Summary = "Cancellation failed",
        Description = "Invalid cancellation data or subscription cannot be cancelled")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.NotFound,
        contentType: "application/json",
        bodyType: typeof(CancelSubscriptionResponse),
        Summary = "Subscription not found",
        Description = "No active subscription found for the specified customer")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.InternalServerError,
        contentType: "application/json",
        bodyType: typeof(CancelSubscriptionResponse),
        Summary = "Server error",
        Description = "An unexpected error occurred during cancellation")]
    [OpenApiSecurity("x-functions-key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = (OpenApiSecurityLocationType)ParameterLocation.Header)]
    public async Task<IActionResult> CancelSubscription(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "subscription/cancel")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancel subscription request triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var command = JsonConvert.DeserializeObject<CancelSubscriptionCommand>(requestBody);

            if (command == null)
            {
                return new BadRequestObjectResult(CancelSubscriptionResponse.FailureResult("Invalid or missing cancellation data."));
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Success
                ? new OkObjectResult(result)
                : new BadRequestObjectResult(result);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in cancel subscription request");
            return new BadRequestObjectResult(CancelSubscriptionResponse.FailureResult("Invalid JSON format"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing cancel subscription request");
            return new ObjectResult(CancelSubscriptionResponse.FailureResult("Internal server error"))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    #endregion

    #region Product and Pricing Management

    [Function("CreateProduct")]
    [OpenApiOperation(
        operationId: "CreateProduct",
        tags: new[] { "Product Management" },
        Summary = "Create subscription product",
        Description = "Creates a new subscription product in the payment system. Products represent the services offered and are required before creating pricing plans.",
        Visibility = OpenApiVisibilityType.Advanced)]
    [OpenApiRequestBody(
        contentType: "application/json",
        bodyType: typeof(CreateProductRequest),
        Required = true,
        Description = "Product creation details including name, description, and metadata",
        Example = typeof(CreateProductRequest))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(ProductResult),
        Summary = "Product created successfully",
        Description = "Product has been created and is ready for price configuration",
        Example = typeof(ProductResult))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(ProductResult),
        Summary = "Product creation failed",
        Description = "Invalid product data or validation failed")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.InternalServerError,
        contentType: "application/json",
        bodyType: typeof(ProductResult),
        Summary = "Server error",
        Description = "An unexpected error occurred during product creation")]
    public async Task<IActionResult> CreateProduct(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "subscription/product/create")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create product request triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var command = JsonConvert.DeserializeObject<CreateProductRequest>(requestBody);

            if (command == null)
            {
                return new BadRequestObjectResult("Invalid or missing product data.");
            }

            var result = await _desolaSubscriptionService.CreateSubscriptionProductAsync(command, cancellationToken);

            return result != null
                ? new OkObjectResult(result)
                : new BadRequestObjectResult(result);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in create product request");
            return new BadRequestObjectResult("Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing create product request");
            return new ObjectResult("Internal server error")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("CreatePrice")]
    [OpenApiOperation(
        operationId: "CreatePrice",
        tags: new[] { "Product Management" },
        Summary = "Create product pricing plan",
        Description = "Creates a new pricing plan for an existing product. Supports various billing intervals, currencies, and pricing models including tiered and per-seat pricing.",
        Visibility = OpenApiVisibilityType.Advanced)]
    [OpenApiRequestBody(
        contentType: "application/json",
        bodyType: typeof(CreatePriceRequest),
        Required = true,
        Description = "Pricing plan configuration including amount, currency, billing interval, and pricing model",
        Example = typeof(CreatePriceRequest))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(PriceResult),
        Summary = "Price created successfully",
        Description = "Pricing plan has been created and is available for subscriptions",
        Example = typeof(PriceResult))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(PriceResult),
        Summary = "Price creation failed",
        Description = "Invalid pricing data or product not found")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.NotFound,
        contentType: "application/json",
        bodyType: typeof(PriceResult),
        Summary = "Product not found",
        Description = "The specified product does not exist")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.InternalServerError,
        contentType: "application/json",
        bodyType: typeof(PriceResult),
        Summary = "Server error",
        Description = "An unexpected error occurred during price creation")]
    public async Task<IActionResult> CreatePrice(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "subscription/price/create")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create price request triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var command = JsonConvert.DeserializeObject<CreatePriceRequest>(requestBody);

            if (command == null)
            {
                return new BadRequestObjectResult("Invalid or missing price data.");
            }

            var result = await _desolaSubscriptionService.CreateProductPriceAsync(command, cancellationToken);

            return new OkObjectResult(result);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in create price request");
            return new BadRequestObjectResult("Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing create price request");
            return new ObjectResult("Internal server error")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("GetProduct")]
    [OpenApiOperation(
        operationId: "GetProduct",
        tags: new[] { "Product Management" },
        Summary = "Retrieve product information",
        Description = "Fetches detailed information about a specific product including metadata, status, and associated pricing plans.",
        Visibility = OpenApiVisibilityType.Advanced)]
    [OpenApiParameter(
        name: "productId",
        In = ParameterLocation.Path,
        Required = true,
        Type = typeof(string),
        Description = "Unique product identifier (starts with 'prod_' for Stripe products)")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(DesolaProductDetail),
        Summary = "Product information retrieved",
        Description = "Product details including all associated pricing plans",
        Example = typeof(DesolaProductDetail))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.NotFound,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Product not found",
        Description = "No product exists with the specified ID")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Invalid product ID",
        Description = "Product ID format is invalid or missing")]
    public async Task<IActionResult> GetProduct(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscription/product/{productId}")] HttpRequest req,
        string productId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get product request triggered for product: {ProductId}", productId);

        try
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                return new BadRequestObjectResult(new { error = "Product ID is required" });
            }

            var result = await _desolaSubscriptionService.GetProductAsync(productId, cancellationToken);

            return result != null
                ? new OkObjectResult(result)
                : new NotFoundObjectResult(new { error = $"Product with ID '{productId}' not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get product request for product: {ProductId}", productId);
            return new ObjectResult(new { error = "Internal server error" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("GetProductPrices")]
    [OpenApiOperation(
        operationId: "GetProductPrices",
        tags: new[] { "Product Management" },
        Summary = "Get product pricing plans",
        Description = "Retrieves all pricing plans associated with a specific product. Supports filtering for active prices only.",
        Visibility = OpenApiVisibilityType.Advanced)]
    [OpenApiParameter(
        name: "productId",
        In = ParameterLocation.Path,
        Required = true,
        Type = typeof(string),
        Description = "Product identifier to retrieve prices for"
        )]
    [OpenApiParameter(
        name: "activeOnly",
        In = ParameterLocation.Query,
        Required = false,
        Type = typeof(bool),
        Description = "Filter to return only active pricing plans (default: true)"
        )]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(List<DesolaPriceDetail>),
        Summary = "Pricing plans retrieved",
        Description = "List of pricing plans for the specified product",
        Example = typeof(List<DesolaPriceDetail>))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.NotFound,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Product not found",
        Description = "No product exists with the specified ID")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Invalid product ID",
        Description = "Product ID format is invalid or missing")]
    public async Task<IActionResult> GetProductPrices(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscription/product/{productId}/prices")] HttpRequest req,
        string productId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get product prices request triggered for product: {ProductId}", productId);

        try
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                return new BadRequestObjectResult(new { error = "Product ID is required" });
            }

            var activeOnly = !bool.TryParse(req.Query["activeOnly"], out var active) || active;

            var result = await _desolaSubscriptionService.GetProductPricesAsync(productId, activeOnly, cancellationToken);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get product prices request for product: {ProductId}", productId);
            return new ObjectResult(new { error = "Internal server error" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    #endregion

    #region Payment Intent Management

    [Function("CreatePaymentIntent")]
    [OpenApiOperation(
        operationId: "CreatePaymentIntent",
        tags: new[] { "Payment Management" },
        Summary = "Create payment setup intent",
        Description = "Creates a setup intent for collecting and storing payment methods for future subscription payments. Used for card validation and storage without immediate charges.",
        Visibility = OpenApiVisibilityType.Advanced)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = (OpenApiSecurityLocationType)ParameterLocation.Header)]
    [OpenApiRequestBody(
        contentType: "application/json",
        bodyType: typeof(CreateSetupIntentCommand),
        Required = true,
        Description = "Setup intent creation details for payment method collection",
        Example = typeof(CreateSetupIntentRequest))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(SetupIntentResult),
        Summary = "Setup intent created successfully",
        Description = "Setup intent is ready for payment method collection",
        Example = typeof(SetupIntentResult))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Invalid setup intent data",
        Description = "Required fields are missing or invalid")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.Unauthorized,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Authentication required",
        Description = "Valid authentication token is required")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.InternalServerError,
        contentType: "application/json",
        bodyType: typeof(object),
        Summary = "Server error",
        Description = "An unexpected error occurred")]
    public async Task<IActionResult> CreatePaymentIntentSetup(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "subscription/paymentIntent/create")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create payment intent request triggered.");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogWarning("Empty request body received");
                return new BadRequestObjectResult(new { Error = "Request body cannot be empty" });
            }

            var isAuthenticated = await IsUserAuthenticated(req);

            if (!isAuthenticated.authenticationStatus)
            {
                return isAuthenticated.authenticationResponse!;
            }

            req.HttpContext.VerifyUserHasAnyAcceptedScope("Files.Read");

            var command = JsonConvert.DeserializeObject<CreateSetupIntentCommand>(requestBody);

            if (command == null)
            {
                return new BadRequestObjectResult("Invalid or missing payment intent data.");
            }
            command.UserId = req.HttpContext.User.Identity is { IsAuthenticated: true } ? req.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

            var result = await _mediator.Send(command, cancellationToken);

            return new OkObjectResult(result);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in create payment intent request");
            return new BadRequestObjectResult("Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing create payment intent request");
            return new ObjectResult("Internal server error")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("GetCustomerPaymentIntents")]
    [OpenApiOperation(
        operationId: "GetCustomerPaymentIntents",
        tags: new[] { "Payment Management" },
        Summary = "Retrieve customer payment intents",
        Description = "Fetches all setup intents and payment methods associated with a customer. Useful for displaying saved payment methods and payment history.",
        Visibility = OpenApiVisibilityType.Advanced)]
    [OpenApiParameter(
        name: "customerId",
        In = ParameterLocation.Query,
        Required = true,
        Type = typeof(string),
        Description = "Stripe customer ID to retrieve payment intents for (must start with 'cus_')"
        )]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(PaymentIntentResult),
        Summary = "Payment intents retrieved",
        Description = "List of setup intents and payment methods for the customer",
        Example = typeof(PaymentIntentResult))]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.NotFound,
        contentType: "application/json",
        bodyType: typeof(ErrorResponse),
        Summary = "Customer not found",
        Description = "No payment intents found for the specified customer")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.BadRequest,
        contentType: "application/json",
        bodyType: typeof(ErrorResponse),
        Summary = "Invalid customer ID",
        Description = "Customer ID format is invalid or missing")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.InternalServerError,
        contentType: "application/json",
        bodyType: typeof(ErrorResponse),
        Summary = "Server error",
        Description = "An unexpected error occurred")]
    public async Task<IActionResult> GetCustomerPaymentIntents(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscription/paymentIntent")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        try
        {
            var customerId = req.Query["customerId"].FirstOrDefault();
            _logger.LogInformation("Get payment intents request for customer: {CustomerId}", customerId);

            if (string.IsNullOrEmpty(customerId))
            {
                return new BadRequestObjectResult(new { error = "Customer parameter is required" });
            }

            if (!customerId.StartsWith("cus_") || customerId.Length < 10)
            {
                _logger.LogWarning("Invalid customer ID format: {CustomerId}", customerId);
                return new BadRequestObjectResult(new ErrorResponse
                {
                    Error = "Invalid customer ID format"
                });
            }

            var query = new GetSetupIntentQuery(customerId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                _logger.LogInformation("No payment intents found for customer: {CustomerId}", customerId);
                return new NotFoundObjectResult(new ErrorResponse
                {
                    Error = $"No payment intents found for customer '{customerId}'",
                });
            }

            _logger.LogInformation("Found {Count} payment intents for customer: {CustomerId}", result.Count(), customerId);

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get customer request");
            return new ObjectResult(new { error = "Internal server error" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    #endregion

    #region Authentication Helper

    private static async Task<(bool authenticationStatus, IActionResult? authenticationResponse)> IsUserAuthenticated(HttpRequest req)
        => await req.HttpContext.AuthenticateAzureFunctionAsync();

    #endregion
}