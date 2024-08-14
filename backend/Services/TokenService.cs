using backend.Services.Interfaces;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Interfaces;
using backend.DTO;
using System.Security.Claims;
using backend.Config;
using Microsoft.Extensions.Options;

namespace backend.Services;

public class TokenService : ITokenService
{
    private readonly ICacheRepository _cache;
    private readonly IJwtService _jwtSvc;
    private readonly JwtConfig _config;
    private readonly ILogger<AuthService> _log;

    public TokenService(ICacheRepository cache, IJwtService jwtSvc, IOptions<JwtConfig> config, ILogger<AuthService> log)
    {
        _cache = cache;
        _jwtSvc = jwtSvc;
        _config = config.Value;
        _log = log;
    }

    public async Task<AuthToken> GetCredentials(User user, SessionCAS sessionCAS)
    {
        var session = await _cache.GetAsync<AuthToken>(SessionKey(user.CASID));
        if (session == null)
        {
            _log.LogInformation($"User {user.CASID} does not have a session, creating new session");
            session = await CreateCredentials(user, sessionCAS);
        }

        var claims = _jwtSvc.ValidateToken(session.AccessToken);
        if (claims == null)
        {
            _log.LogInformation($"User {user.CASID} has an invalid access token, creating new session");
            await RemoveSessionCache(user.CASID);
            string accessToken = _jwtSvc.CreateToken(user, sessionCAS);

            var credentials = new AuthToken
            {
                AccessToken = accessToken,
                RefreshToken = session.RefreshToken,
                ExpiresIn = DateTime.UtcNow.AddSeconds(_config.AccessTTL)
            };
            await _cache.SetAsync(SessionKey(user.CASID), credentials, TimeSpan.FromSeconds(_config.AccessTTL));

            return credentials;
        }

        return session;
    }

    public async Task<AuthToken?> RefreshToken(string refreshToken)
    {
        var user = await _cache.GetAsync<User>(RefreshKey(refreshToken));
        if (user == null) return null;

        var authToken = await GetSessionCache(user.CASID);
        if (authToken == null) return null;

        var claims = _jwtSvc.ValidateToken(authToken.AccessToken);
        if (claims == null) return null;

        await RemoveSessionCache(user.CASID);

        var oldSession = new SessionCAS
        {
            Token = "",
            UserID = claims.FindFirstValue("userCASID") ?? throw new InvalidOperationException("User CASID is missing"),
            Email = claims.FindFirstValue("casEmail") ?? throw new InvalidOperationException("casEmail is missing"),
            Role = claims.FindFirstValue("casRole") ?? throw new InvalidOperationException("casRole is missing"),
            ExpiresAt = DateTime.UtcNow,
        };

        string accessToken = _jwtSvc.CreateToken(user, oldSession);
        var credentials = new AuthToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = DateTime.UtcNow.AddMinutes(_config.AccessTTL)
        };

        await _cache.SetAsync(SessionKey(user.CASID), credentials, TimeSpan.FromSeconds(_config.AccessTTL));

        return credentials;
    }

    public async Task<Credentials?> ValidateToken(string accessToken)
    {
        var claims = _jwtSvc.ValidateToken(accessToken);
        if (claims == null) return null;

        var expiryString = claims.FindFirstValue("exp");
        long.TryParse(expiryString, out long unixTimestamp);
        var expiry = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
        if (expiry < DateTime.Now.ToUniversalTime())
        {
            _log.LogInformation("Token has expired");
            return null;
        }

        var userCASID = claims.FindFirstValue("userCASID") ?? throw new InvalidOperationException("User CASID is missing");
        var email = claims.FindFirstValue("casEmail") ?? throw new InvalidOperationException("casEmail is missing");
        var role = claims.FindFirstValue("casRole") ?? throw new InvalidOperationException("casRole is missing");

        var session = await _cache.GetAsync<AuthToken>(SessionKey(userCASID));
        if (session == null) return null;

        return new Credentials
        {
            UserID = userCASID,
            Email = email,
            Role = role,
        };
    }

    public async Task<AuthToken?> GetSessionCache(string? userCASID)
    {
        return await _cache.GetAsync<AuthToken>(SessionKey(userCASID));
    }

    public async Task RemoveSessionCache(string? userCASID)
    {
        await _cache.RemoveAsync(SessionKey(userCASID));
    }

    public async Task RemoveRefreshCache(string? refreshToken)
    {
        await _cache.RemoveAsync(RefreshKey(refreshToken));
    }

    private async Task<AuthToken> CreateCredentials(User user, SessionCAS session)
    {
        string accessToken = _jwtSvc.CreateToken(user, session);
        string refreshToken = CreateRefreshToken();

        await _cache.SetAsync(RefreshKey(refreshToken), user, session.ExpiresAt - DateTime.Now);

        var credentials = new AuthToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = DateTime.UtcNow.AddSeconds(_config.AccessTTL)
        };

        await _cache.SetAsync(SessionKey(user.CASID), credentials, TimeSpan.FromSeconds(_config.AccessTTL));

        return credentials;
    }

    private string CreateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    private string RefreshKey(string? refreshToken)
    {
        return $"{_config.Issuer}_refresh_{refreshToken}";
    }

    private string SessionKey(string? userCASID)
    {
        return $"{_config.Issuer}_session_{userCASID}";
    }
}
