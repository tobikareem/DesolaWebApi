using System.Net;
using System.Text.Json.Serialization;

namespace Desola.Common.Exceptions;

public class SkyScannerApiException : ApiException
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }

    public SkyScannerApiException(HttpStatusCode statusCode, string errorCode, string errorMessage)
        : base($"SkyScanner API error: {statusCode} - {errorCode}: {errorMessage}")
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}

public class SkyScannerErrorResponse
{
    [JsonPropertyName("errors")]
    public List<SkyScannerError> Errors { get; set; } = new();
}

public class SkyScannerError : ApiKnownError { }