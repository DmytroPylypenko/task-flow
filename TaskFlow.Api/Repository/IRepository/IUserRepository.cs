using TaskFlow.Api.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Api.Repository.IRepository;

/// <summary>
/// Defines data access methods for the User entity.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Checks if a user with the specified email already exists.
    /// </summary>
    Task<bool> UserExistsAsync(string email);

    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    Task AddUserAsync(User user);

    /// <summary>
    /// Finds a user by their email address.
    /// </summary>
    Task<User?> FindUserByEmailAsync(string email);
}