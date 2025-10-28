using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;

namespace TaskFlow.Api.Repository;

/// <summary>
/// Implements data access for the Board entity using Entity Framework Core.
/// </summary>
public class BoardRepository :  IBoardRepository
{
    private readonly ApplicationDbContext _context;

    public BoardRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<Board>> GetBoardsByUserIdAsync(int userId)
    {
        return await _context.Boards
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }
}