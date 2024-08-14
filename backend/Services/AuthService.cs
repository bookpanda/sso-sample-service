using System.Net;
using backend.DTO;
using backend.Exceptions;
using backend.Services.Interfaces;

namespace backend.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _log;

    public AuthService(IUserService userService, ITokenService tokenService, ILogger<AuthService> log)
    {
        _userService = userService;
        _tokenService = tokenService;
        _log = log;
    }

    public async Task<AuthToken> AuthenticateSSO(SessionCAS sessionCAS)
    {
        Models.User? user;
        try
        {
            user = await _userService.FindOne(sessionCAS.UserID);
            if (user == null)
            {
                _log.LogInformation($"User with id {sessionCAS.UserID} not found, creating new user");

                var newUser = new CreateUserDTO { CASID = sessionCAS.UserID };
                var createdUser = await _userService.Create(newUser);

                var newAuthToken = await _tokenService.GetCredentials(createdUser, sessionCAS);
                return newAuthToken;
            }
        }
        catch (ServiceException ex)
        {
            _log.LogError(ex, $"Error finding user with CASID {sessionCAS.UserID}");
            throw;
        }

        try
        {
            var authToken = await _tokenService.GetCredentials(user!, sessionCAS);
            return authToken;
        }
        catch (ServiceException ex)
        {
            _log.LogError(ex, $"Error getting credentials for user {sessionCAS.UserID}");
            throw;
        }
    }
}