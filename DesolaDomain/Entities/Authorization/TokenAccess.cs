using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Authorization;

public class TokenAccess
{
    [JsonPropertyName("type")]
    public string Type;

    [JsonPropertyName("username")]
    public string Username;

    [JsonPropertyName("application_name")]
    public string ApplicationName;

    [JsonPropertyName("client_id")]
    public string ClientId;

    [JsonPropertyName("token_type")]
    public string TokenType;

    [JsonPropertyName("access_token")]
    public string AccessToken;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn;

    [JsonPropertyName("state")]
    public string State;

    [JsonPropertyName("scope")]
    public string Scope;
}
