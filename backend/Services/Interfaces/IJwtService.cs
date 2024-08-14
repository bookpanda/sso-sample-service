
using System.Security.Claims;
using backend.DTO;
using backend.Models;

namespace backend.Interfaces;

public interface IJwtService
{
    string CreateToken(User user, SessionCAS session);
    ClaimsPrincipal? ValidateToken(string token);
}