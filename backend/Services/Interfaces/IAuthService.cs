using backend.DTO;

namespace backend.Services.Interfaces;

public interface IAuthService
{
    Task<AuthToken> AuthenticateSSO(SessionCAS session);
}