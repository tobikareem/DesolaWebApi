using System.Net;
using System.Text.Json.Serialization;

namespace Desola.Common.Exceptions;

public class GoogleErrorResponse
{

    [JsonPropertyName("errors")]
    public Dictionary<string, string> Errors { get; set; } = new();

    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "Errors";
}


public class GoogleApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public GoogleErrorResponse ErrorResponse { get; }

    public string FirstErrorKey => ErrorResponse?.Errors?.Keys.FirstOrDefault() ?? "Unknown";
    public string FirstErrorMessage => ErrorResponse?.Errors?.Values.FirstOrDefault() ?? "No details available";

    public GoogleApiException(HttpStatusCode statusCode, GoogleErrorResponse errorResponse)
        : base(errorResponse?.Message ?? "Google API error")
    {
        StatusCode = statusCode;
        ErrorResponse = errorResponse;
    }

    public GoogleApiException(string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = HttpStatusCode.InternalServerError;
        ErrorResponse = new GoogleErrorResponse
        {
            Status = false,
            Message = message,
            Errors = new Dictionary<string, string>
            {
                { "internal", message }
            }
        };
    }
}