using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using backend.Interfaces;
using backend.Models;
using backend.Config;
using Microsoft.Extensions.Options;
using backend.DTO;

namespace backend.Services;

public class JwtService : IJwtService
{
    private readonly JwtConfig _config;
    private readonly SymmetricSecurityKey _key;
    private readonly ILogger<JwtService> _log;

    public JwtService(IOptions<JwtConfig> config, ILogger<JwtService> log)
    {
        _config = config.Value;
        _log = log;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config.Secret ?? throw new InvalidOperationException("JWT Secret is missing")));
    }

    public string CreateToken(User user, SessionCAS session)
    {
        try
        {
            _log.LogInformation($"JWTASS {session.Email}, {session.Role}");
            var claims = new List<Claim>
            {
                new Claim("userID", user.ID ?? throw new InvalidOperationException("User Name is missing")),
                new Claim("userCASID", session.UserID ?? throw new InvalidOperationException("User CASID is missing")),
                new Claim("casEmail", session.Email ?? throw new InvalidOperationException("Email is missing")),
                new Claim("casRole", session.Role ?? throw new InvalidOperationException("Role is missing")),
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var accessTTL = _config.AccessTTL;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _config.Issuer,
                IssuedAt = DateTime.Now,
                Expires = DateTime.Now.AddSeconds(accessTTL),
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error creating JWT token");
            throw;
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var claims = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidIssuer = _config.Issuer,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
            }, out var validatedToken);

            return claims;
        }
        catch (Exception ex)
        {
            _log.LogInformation(ex, "Error validating JWT token");
            return null;
        }
    }
}
