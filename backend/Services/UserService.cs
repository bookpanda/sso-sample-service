using System.Net;
using backend.DTO;
using backend.Exceptions;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly ILogger<UserService> _log;

    public UserService(IUserRepository repo, ILogger<UserService> log)
    {
        _repo = repo;
        _log = log;
    }

    public async Task<User?> FindOne(string casid)
    {
        try
        {
            var user = await _repo.FindOne(casid);
            if (user == null)
            {
                _log.LogInformation($"User with CASID {casid} not found");
                return null;
            }

            return user;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, $"Error finding user with CASID {casid}");
            throw new ServiceException("Error finding user", HttpStatusCode.InternalServerError, ex);
        }
    }

    public async Task<User> Create(CreateUserDTO user)
    {
        var newUser = new User
        {
            CASID = user.CASID,
        };

        try
        {
            await _repo.Create(newUser);
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error creating user", HttpStatusCode.InternalServerError, ex);
        }

        return newUser;
    }
}