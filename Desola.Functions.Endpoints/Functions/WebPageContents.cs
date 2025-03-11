using System.Text.Json;
using DesolaServices.Commands.Queries;
using DesolaServices.Commands.Requests;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using DesolaDomain.Entities.Pages;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Desola.Functions.Endpoints.Functions;
public class WebPageContents
{
    private readonly ILogger<WebPageContents> _logger;
    private readonly IMediator _mediator;

    public WebPageContents(ILogger<WebPageContents> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Add a new page section
    /// </summary>
    [Function("AddPageSection")]
    public async Task<HttpResponseData> AddSection(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "page/addSection")] HttpRequestData req)
    {
        _logger.LogInformation("Processing request to add a new page section." + req.Url);

        HttpResponseData response;
        try
        {
            // Read and parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var section = JsonSerializer.Deserialize<WebSection>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate parsed entity
            if (section == null)
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid request payload.");
                return response;
            }


            await _mediator.Send(new InsertWebSectionCommand(section));

            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"{section.RowKey} web sections added successfully.");
            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON Deserialization Error: {ex.Message}");
            response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("Invalid JSON format.");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected Error: {ex.Message}");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error.");
            return response;
        }
    }

    /// <summary>
    /// Retrieve a specific page section
    /// </summary>
    [Function("GetPageSection")]
    public async Task<HttpResponseData> GetSection(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "page/{partitionKey}/{rowKey}")] HttpRequestData req,
        string partitionKey, string rowKey)
    {
        _logger.LogInformation($"Fetching section with PartitionKey: {partitionKey}, RowKey: {rowKey}.");

        HttpResponseData response;
        try
        {
            var section = await _mediator.Send(new GetWebSectionQuery(partitionKey, rowKey));
            if (section == null)
            {
                response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteStringAsync($"Section with RowKey '{rowKey}' not found.");
                return response;
            }

            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(section);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving section: {ex.Message}");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error.");
            return response;
        }
    }

    /// <summary>
    /// Update an existing page section
    /// </summary>
    [Function("UpdatePageSection")]
    public async Task<HttpResponseData> UpdateSection(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "page/updateSection")] HttpRequestData req)
    {
        _logger.LogInformation("Processing request to update a page section.");

        HttpResponseData response;
        try
        {
            // Read and parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var section = JsonSerializer.Deserialize<WebSection>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate parsed entity
            if (section == null)
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid request payload.");
                return response;
            }

            // Execute CQRS command
            await _mediator.Send(new UpdateWebSectionCommand(section));

            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Web section '{section.RowKey}' updated successfully.");
            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON Deserialization Error: {ex.Message}");
            response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("Invalid JSON format.");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected Error: {ex.Message}");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Internal server error.");
            return response;
        }
    }
}