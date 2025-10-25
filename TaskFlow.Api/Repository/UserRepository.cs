using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Api.Repository;

/// <summary>
/// Implements data access for the User entity using Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<bool> UserExistsAsync(string email)
    {
        return _context.Users.AnyAsync(u => u.Email == email);
    }

    /// <inheritdoc />
    public async Task AddUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public Task<User?> FindUserByEmailAsync(string email)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}