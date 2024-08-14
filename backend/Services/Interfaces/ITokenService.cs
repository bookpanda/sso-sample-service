using backend.DTO;
using backend.Models;

namespace backend.Services.Interfaces;

public interface ITokenService
{
    Task<AuthToken> GetCredentials(User user, SessionCAS sessionCAS);
    Task<AuthToken?> RefreshToken(string refreshToken);
    Task<Credentials?> ValidateToken(string accessToken);
    Task<AuthToken?> GetSessionCache(string? userID);
    Task RemoveSessionCache(string key);
    Task RemoveRefreshCache(string key);
}