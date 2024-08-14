using backend.DTO;
using backend.Models;

namespace backend.Services.Interfaces;

public interface IUserService
{
    Task<User?> FindOne(string id);
    Task<User> Create(CreateUserDTO user);
}