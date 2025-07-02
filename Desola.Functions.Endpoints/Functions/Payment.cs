using DesolaServices.Commands.Queries.Payment;
using DesolaServices.Commands.Requests;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
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

namespace Desola.Functions.Endpoints.Functions;

public class Payment
{
    private readonly ILogger<Payment> _logger;
    private readonly IMediator _mediator;

    public Payment(ILogger<Payment> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }


    [Function("CustomerSignup")]
    [OpenApiOperation("CustomerSignup", tags: new[] { "Customer Management" })]
    [OpenApiRequestBody("application/json", typeof(NewUserSignUpCommand), Required = true, Description = "Customer signup information")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CustomerSignupResponse), Description = "Customer created successfully")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(CustomerSignupResponse), Description = "Validation failed or customer already exists")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(CustomerSignupResponse), Description = "Internal server error")]
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

            command.CustomerId = req.HttpContext.User.Identity is { IsAuthenticated: true } ? req.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null; // 78af2b87-6e98-4eec-91ba-2d12d36e71c3

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
    [OpenApiOperation("UpdateCustomer", tags: new[] { "Customer Management" })]
    [OpenApiRequestBody("application/json", typeof(UpdateCustomerCommand), Required = true, Description = "Customer update information")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CustomerUpdateResponse), Description = "Customer updated successfully")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(CustomerUpdateResponse), Description = "Validation failed")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(CustomerUpdateResponse), Description = "Customer not found")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(CustomerUpdateResponse), Description = "Internal server error")]
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
    [OpenApiOperation("GetCustomer", tags: new[] { "Customer Management" })]
    [OpenApiParameter(name: "email", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Customer email address")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CustomerDto), Description = "Customer found")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(object), Description = "Customer not found")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(object), Description = "Invalid email provided")]
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
    [OpenApiOperation("UpdateCustomerField", tags: new[] { "Customer Management" })]
    [OpenApiParameter(name: "email", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer email address")]
    [OpenApiParameter(name: "field", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Field to update (fullname, phone, currency, airport)")]
    [OpenApiRequestBody("application/json", typeof(string), Required = true, Description = "New field value")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CustomerUpdateResponse), Description = "Field updated successfully")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(CustomerUpdateResponse), Description = "Invalid field or value")]
    [OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(CustomerUpdateResponse), Description = "Customer not found")]
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

    //[Function("CreateSubscription")]
    //[OpenApiOperation("CreateSubscription", tags: new[] { "Subscription Management" })]
    //[OpenApiRequestBody("application/json", typeof(CreateSubscriptionCommand), Required = true, Description = "Subscription creation information")]
    //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CreateSubscriptionResponse), Description = "Subscription created successfully")]
    //[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(CreateSubscriptionResponse), Description = "Validation failed or subscription creation failed")]
    //[OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(CreateSubscriptionResponse), Description = "Customer not found")]
    //[OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(CreateSubscriptionResponse), Description = "Internal server error")]
    //public async Task<IActionResult> CreateSubscription(
    //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "subscription/create")] HttpRequest req,
    //    CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Create subscription request triggered.");

    //    try
    //    {
    //        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
    //        var command = JsonConvert.DeserializeObject<CreateSubscriptionCommand>(requestBody);

    //        if (command == null)
    //        {
    //            return new BadRequestObjectResult(CreateSubscriptionResponse.FailureResult("Invalid or missing subscription data."));
    //        }

    //        var result = await _mediator.Send(command, cancellationToken);

    //        return result.Success switch
    //        {
    //            true => new OkObjectResult(result),
    //            false when result.Message.Contains("not found") => new NotFoundObjectResult(result),
    //            false => new BadRequestObjectResult(result)
    //        };
    //    }
    //    catch (JsonException ex)
    //    {
    //        _logger.LogError(ex, "Invalid JSON in create subscription request");
    //        return new BadRequestObjectResult(CreateSubscriptionResponse.FailureResult("Invalid JSON format"));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error processing create subscription request");
    //        return new ObjectResult(CreateSubscriptionResponse.FailureResult("Internal server error"))
    //        {
    //            StatusCode = StatusCodes.Status500InternalServerError
    //        };
    //    }
    //}

    //[Function("CancelSubscription")]
    //[OpenApiOperation("CancelSubscription", tags: new[] { "Subscription Management" })]
    //[OpenApiRequestBody("application/json", typeof(CancelSubscriptionCommand), Required = true, Description = "Subscription cancellation information")]
    //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CancelSubscriptionResponse), Description = "Subscription cancelled successfully")]
    //[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(CancelSubscriptionResponse), Description = "Validation failed or cancellation failed")]
    //[OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(CancelSubscriptionResponse), Description = "Subscription not found")]
    //public async Task<IActionResult> CancelSubscription(
    //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "subscription/cancel")] HttpRequest req,
    //    CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Cancel subscription request triggered.");

    //    try
    //    {
    //        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
    //        var command = JsonConvert.DeserializeObject<CancelSubscriptionCommand>(requestBody);

    //        if (command == null)
    //        {
    //            return new BadRequestObjectResult(CancelSubscriptionResponse.FailureResult("Invalid or missing cancellation data."));
    //        }

    //        var result = await _mediator.Send(command, cancellationToken);

    //        return result.Success switch
    //        {
    //            true => new OkObjectResult(result),
    //            false when result.Message.Contains("not found") => new NotFoundObjectResult(result),
    //            false => new BadRequestObjectResult(result)
    //        };
    //    }
    //    catch (JsonException ex)
    //    {
    //        _logger.LogError(ex, "Invalid JSON in cancel subscription request");
    //        return new BadRequestObjectResult(CancelSubscriptionResponse.FailureResult("Invalid JSON format"));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error processing cancel subscription request");
    //        return new ObjectResult(CancelSubscriptionResponse.FailureResult("Internal server error"))
    //        {
    //            StatusCode = StatusCodes.Status500InternalServerError
    //        };
    //    }
    //}


    //#region Payment Management

    //[Function("CreatePaymentIntent")]
    //[OpenApiOperation("CreatePaymentIntent", tags: new[] { "Payment Management" })]
    //[OpenApiRequestBody("application/json", typeof(CreatePaymentIntentCommand), Required = true, Description = "Payment intent creation information")]
    //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CreatePaymentIntentResponse), Description = "Payment intent created successfully")]
    //[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(CreatePaymentIntentResponse), Description = "Validation failed or payment intent creation failed")]
    //[OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(CreatePaymentIntentResponse), Description = "Internal server error")]
    //public async Task<IActionResult> CreatePaymentIntent(
    //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "payment/intent/create")] HttpRequest req,
    //    CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Create payment intent request triggered.");

    //    try
    //    {
    //        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
    //        var command = JsonConvert.DeserializeObject<CreatePaymentIntentCommand>(requestBody);

    //        if (command == null)
    //        {
    //            return new BadRequestObjectResult(CreatePaymentIntentResponse.FailureResult("Invalid or missing payment intent data."));
    //        }

    //        var result = await _mediator.Send(command, cancellationToken);

    //        return result.Success
    //            ? new OkObjectResult(result)
    //            : new BadRequestObjectResult(result);
    //    }
    //    catch (JsonException ex)
    //    {
    //        _logger.LogError(ex, "Invalid JSON in create payment intent request");
    //        return new BadRequestObjectResult(CreatePaymentIntentResponse.FailureResult("Invalid JSON format"));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error processing create payment intent request");
    //        return new ObjectResult(CreatePaymentIntentResponse.FailureResult("Internal server error"))
    //        {
    //            StatusCode = StatusCodes.Status500InternalServerError
    //        };
    //    }
    //}

    //[Function("ConfirmPaymentIntent")]
    //[OpenApiOperation("ConfirmPaymentIntent", tags: new[] { "Payment Management" })]
    //[OpenApiRequestBody("application/json", typeof(ConfirmPaymentIntentCommand), Required = true, Description = "Payment intent confirmation information")]
    //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ConfirmPaymentIntentResponse), Description = "Payment intent confirmed successfully")]
    //[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(ConfirmPaymentIntentResponse), Description = "Validation failed or confirmation failed")]
    //[OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof(ConfirmPaymentIntentResponse), Description = "Payment intent not found")]
    //public async Task<IActionResult> ConfirmPaymentIntent(
    //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "payment/intent/confirm")] HttpRequest req,
    //    CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Confirm payment intent request triggered.");

    //    try
    //    {
    //        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
    //        var command = JsonConvert.DeserializeObject<ConfirmPaymentIntentCommand>(requestBody);

    //        if (command == null)
    //        {
    //            return new BadRequestObjectResult(ConfirmPaymentIntentResponse.FailureResult("Invalid or missing confirmation data."));
    //        }

    //        var result = await _mediator.Send(command, cancellationToken);

    //        return result.Success switch
    //        {
    //            true => new OkObjectResult(result),
    //            false when result.Message.Contains("not found") => new NotFoundObjectResult(result),
    //            false => new BadRequestObjectResult(result)
    //        };
    //    }
    //    catch (JsonException ex)
    //    {
    //        _logger.LogError(ex, "Invalid JSON in confirm payment intent request");
    //        return new BadRequestObjectResult(ConfirmPaymentIntentResponse.FailureResult("Invalid JSON format"));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error processing confirm payment intent request");
    //        return new ObjectResult(ConfirmPaymentIntentResponse.FailureResult("Internal server error"))
    //        {
    //            StatusCode = StatusCodes.Status500InternalServerError
    //        };
    //    }
    //}
    //[Function("StripeWebhook")]
    //[OpenApiOperation("StripeWebhook", tags: new[] { "Webhook Management" })]
    //[OpenApiRequestBody("application/json", typeof(object), Required = true, Description = "Stripe webhook payload")]
    //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(object), Description = "Webhook processed successfully")]
    //[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(object), Description = "Invalid webhook signature or payload")]
    //[OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(object), Description = "Internal server error")]
    //public async Task<IActionResult> StripeWebhook(
    //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "webhook/stripe")] HttpRequest req,
    //    CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Stripe webhook request triggered.");

    //    try
    //    {
    //        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
    //        var signature = req.Headers["Stripe-Signature"].FirstOrDefault();

    //        if (string.IsNullOrEmpty(signature))
    //        {
    //            _logger.LogWarning("Missing Stripe signature in webhook request");
    //            return new BadRequestObjectResult(new { error = "Missing Stripe signature" });
    //        }

    //        var command = new ProcessStripeWebhookCommand
    //        {
    //            Payload = requestBody,
    //            Signature = signature
    //        };

    //        var result = await _mediator.Send(command, cancellationToken);

    //        return result.Success
    //            ? new OkObjectResult(new { message = "Webhook processed successfully" })
    //            : new BadRequestObjectResult(new { error = result.ErrorMessage });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error processing Stripe webhook");
    //        return new ObjectResult(new { error = "Internal server error" })
    //        {
    //            StatusCode = StatusCodes.Status500InternalServerError
    //        };
    //    }
    //}

    //#endregion

    //#region Health Check

    //[Function("PaymentHealthCheck")]
    //[OpenApiOperation("PaymentHealthCheck", tags: new[] { "Health Check" })]
    //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(object), Description = "Service is healthy")]
    //public async Task<IActionResult> PaymentHealthCheck(
    //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "payment/health")] HttpRequest req)
    //{
    //    _logger.LogInformation("Payment health check triggered.");

    //    try
    //    {
    //        // You could add more sophisticated health checks here
    //        // - Database connectivity
    //        // - Stripe API connectivity
    //        // - Cache service status

    //        var healthStatus = new
    //        {
    //            status = "healthy",
    //            timestamp = DateTime.UtcNow,
    //            service = "Desola Payment Service",
    //            version = "1.0.0",
    //            dependencies = new
    //            {
    //                database = "healthy",
    //                stripe = "healthy",
    //                cache = "healthy"
    //            }
    //        };

    //        return new OkObjectResult(healthStatus);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error during health check");

    //        var healthStatus = new
    //        {
    //            status = "unhealthy",
    //            timestamp = DateTime.UtcNow,
    //            service = "Desola Payment Service",
    //            error = ex.Message
    //        };

    //        return new ObjectResult(healthStatus)
    //        {
    //            StatusCode = StatusCodes.Status503ServiceUnavailable
    //        };
    //    }
    //}

    //#endregion

    private static async Task<(bool authenticationStatus, IActionResult? authenticationResponse)> IsUserAuthenticated(HttpRequest req) => await req.HttpContext.AuthenticateAzureFunctionAsync();

}