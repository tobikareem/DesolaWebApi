using System.Text.Json.Serialization;

namespace Desola.Common.Exceptions;

public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception innerException) : base(message, innerException) { }
}

public class ApiTimeoutException : ApiException
{
    public ApiTimeoutException(string message) : base(message) { }
    public ApiTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}

public class ApiAuthenticationException : ApiException
{
    public ApiAuthenticationException(string message) : base(message) { }
    public ApiAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}

public class ApiResourceNotFoundException : ApiException
{
    public ApiResourceNotFoundException(string message) : base(message) { }
    public ApiResourceNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

public class ApiResponseFormatException : ApiException
{
    public ApiResponseFormatException(string message) : base(message) { }
    public ApiResponseFormatException(string message, Exception innerException) : base(message, innerException) { }
}

public class ApiKnownError
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
