using backend.Models;

namespace backend.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> FindOne(string id);
    Task<User> Create(User todo);
}