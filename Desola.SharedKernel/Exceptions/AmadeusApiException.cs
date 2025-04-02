
using System.Net;
using System.Text.Json.Serialization;

namespace Desola.Common.Exceptions;

public class AmadeusErrorResponse
{
    [JsonPropertyName("errors")]
    public List<AmadeusError> Errors { get; set; } = new();
}

public class AmadeusError
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Unknown Error";

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = "No details available";
}

public class AmadeusApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public AmadeusErrorResponse ErrorResponse { get; }
    public string ErrorTitle => ErrorResponse?.Errors?.FirstOrDefault()?.Title ?? "Unknown Error";
    public string ErrorDetail => ErrorResponse?.Errors?.FirstOrDefault()?.Detail ?? "No details available";

    public AmadeusApiException(HttpStatusCode statusCode, AmadeusErrorResponse errorResponse)
        : base(errorResponse.Errors.FirstOrDefault()?.Detail ?? "Amadeus API error")
    {
        StatusCode = statusCode;
        ErrorResponse = errorResponse;
    }

    public AmadeusApiException(string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = HttpStatusCode.InternalServerError;
        ErrorResponse = new AmadeusErrorResponse
        {
            Errors = new List<AmadeusError>
            {
                new()
                {
                    Status = 500,
                    Title = "INTERNAL_ERROR",
                    Detail = message
                }
            }
        };
    }
}