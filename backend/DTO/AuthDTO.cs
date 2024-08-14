using System.Text.Json.Serialization;

namespace backend.DTO;

public class AuthToken
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; set; }
    [JsonPropertyName("expires_in")]
    public required DateTime ExpiresIn { get; set; }
}

public class Credentials
{
    [JsonPropertyName("user_id")]
    public required string UserID { get; set; }
    [JsonPropertyName("email")]
    public required string Email { get; set; }
    [JsonPropertyName("role")]
    public required string Role { get; set; }
}

public class RefreshTokenBody
{
    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; set; }
}

public class SignoutBody
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }
}

public class SessionCAS
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
    [JsonPropertyName("user_id")]
    public required string UserID { get; set; }
    [JsonPropertyName("email")]
    public required string Email { get; set; }
    [JsonPropertyName("role")]
    public required string Role { get; set; }
    [JsonPropertyName("expires_at")]
    public required DateTime ExpiresAt { get; set; }
}