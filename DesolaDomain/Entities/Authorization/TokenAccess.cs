using System.Text.Json.Serialization;
using Desola.Common;

namespace DesolaDomain.Entities.Authorization;

public class TokenAccess
{
    private const long TokenBuffer = 10000L; // 10 seconds
    private long _expiresAt;

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("application_name")]
    public string ApplicationName { get; set; }

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    public virtual string BearerToken => $"Bearer {AccessToken}";

    // Checks if this access token needs a refresh.
    public bool NeedsRefresh()
    {
        return string.IsNullOrEmpty(AccessToken) || (DateTimeHelper.CurrentUnixTimeMillis() + TokenBuffer) > _expiresAt;
    }

    public void RefreshToken(string newAccessToken, int expiresIn)
    {
        AccessToken = newAccessToken;
        ExpiresIn = expiresIn;
        _expiresAt = DateTimeHelper.CurrentUnixTimeMillis() + expiresIn * 1000;
    }
}
