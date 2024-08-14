using Microsoft.AspNetCore.Mvc;
using backend.Services.Interfaces;
using backend.Config;
using backend.DTO;
using backend.Exceptions;
using Microsoft.Extensions.Options;

namespace backend.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly SSOConfig _config;
    private readonly IAuthService _authSvc;
    private readonly ITokenService _tokenSvc;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthController> _log;

    public AuthController(IOptions<SSOConfig> config, IAuthService authSvc, ITokenService tokenSvc, HttpClient httpClient, ILogger<AuthController> log)
    {
        _config = config.Value;
        _authSvc = authSvc;
        _tokenSvc = tokenSvc;
        _httpClient = httpClient;
        _log = log;
    }

    [HttpGet("auth-sso")]
    public async Task<IActionResult> AuthenticateSSO([FromQuery] string ticket)
    { // only after redirect from CAS
        try
        {
            var response = await _httpClient.GetAsync(_config.Authority + "/api/v1/auth/validate-st?ticket=" + ticket + "&service=" + _config.Service);
            if (!response.IsSuccessStatusCode) return Unauthorized("Invalid ticket");

            var session = await response.Content.ReadFromJsonAsync<SessionCAS>();
            if (session == null) return Unauthorized("Invalid session");

            var authToken = await _authSvc.AuthenticateSSO(session);

            return Ok(authToken);
        }
        catch (ServiceException ex)
        {
            _log.LogError(ex, "Error authenticating SSO");
            return StatusCode((int)ex.StatusCode, ex.Message);
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenBody req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var credentials = await _tokenSvc.RefreshToken(req.RefreshToken);
            if (credentials == null) return Unauthorized("Invalid refresh token");

            return Ok(credentials);
        }
        catch (ServiceException ex)
        {
            _log.LogError(ex, "Error refreshing token");
            return StatusCode((int)ex.StatusCode, ex.Message);
        }
    }

    [HttpGet("validate")]
    public async Task<IActionResult> Validate([FromHeader(Name = "Authorization")] string authHeader)
    {
        var accessToken = authHeader?.Split(" ").Last();
        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequest("Authorization header is missing.");
        }

        try
        {
            var creds = await _tokenSvc.ValidateToken(accessToken);
            if (creds == null)
            {
                _log.LogError("Cannot find credentials");
                return NotFound("Cannot find credentials");
            }

            return Ok(creds);
        }
        catch (ServiceException ex)
        {
            _log.LogError(ex, "Error validating token");
            return StatusCode((int)ex.StatusCode, ex.Message);
        }
    }

    [HttpPost("signout")]
    public async Task<IActionResult> Signout([FromBody] SignoutBody req)
    {
        try
        {
            var creds = await _tokenSvc.ValidateToken(req.AccessToken);
            if (creds == null)
            {
                _log.LogError("Cannot find credentials");
                return NotFound("Cannot find credentials");
            }

            var authToken = await _tokenSvc.GetSessionCache(creds.UserID);
            if (authToken == null)
            {
                _log.LogError("Cannot find session cache");
                return NotFound("Cannot find session cache");
            }

            await _tokenSvc.RemoveSessionCache(creds.UserID);
            await _tokenSvc.RemoveRefreshCache(authToken.RefreshToken);

            return Ok(creds);
        }
        catch (ServiceException ex)
        {
            _log.LogError(ex, "Error validating token");
            return StatusCode((int)ex.StatusCode, ex.Message);
        }
    }

    [HttpPost("sso-signout")]
    public async Task<IActionResult> SSOSignout([FromQuery(Name = "user_id")] string userID)
    {
        try
        {
            var response = await _httpClient.GetAsync(_config.Authority + "/api/v1/auth/check-session?service=" + _config.Service);
            if (response.IsSuccessStatusCode) return Unauthorized("SSO session still valid");

            // SSO session no longer valid, remove cache
            var authToken = await _tokenSvc.GetSessionCache(userID);
            if (authToken == null)
            {
                _log.LogError($"Cannot find session cache for user ID {userID}");
                return NotFound("Cannot find session cache");
            }

            await _tokenSvc.RemoveSessionCache(userID);
            await _tokenSvc.RemoveRefreshCache(authToken.RefreshToken);

            return Ok();
        }
        catch (ServiceException ex)
        {
            _log.LogError(ex, "Error validating token");
            return StatusCode((int)ex.StatusCode, ex.Message);
        }
    }

    // private void SetHttpCookie(string key, string value, DateTime? expiry = null)
    // {
    //     var cookieOptions = new CookieOptions
    //     {
    //         HttpOnly = true,
    //         SameSite = SameSiteMode.None,
    //         Domain = "localhost",
    //         Path = "/",
    //     };

    //     if (expiry.HasValue)
    //     {
    //         cookieOptions.Expires = expiry.Value;
    //     }

    //     Response.Cookies.Append(key, value, cookieOptions);
    // }

    // private string? GetHttpCookie(string key)
    // {
    //     if (Request.Cookies.TryGetValue(key, out string? cookieValue))
    //     {
    //         return cookieValue;
    //     }

    //     return null;
    // }
}