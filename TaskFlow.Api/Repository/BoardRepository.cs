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
            .OrderByDescending(b  => b.Id)
            .ToListAsync();
    }
    
    /// <inheritdoc />
    public async Task<Board?> GetBoardByIdAsync(int boardId, int userId)
    {
        return await _context.Boards
            .Include(b => b.Columns)
            .ThenInclude(c => c.Tasks.OrderBy(t => t.Position))
            .FirstOrDefaultAsync(b => b.Id == boardId && b.UserId == userId);
    }
    
    /// <inheritdoc />
    public async Task<Board> CreateBoardAsync(Board board)
    {
        board.Columns.Add(new Column { Name = "To Do" });
        board.Columns.Add(new Column { Name = "In Progress" });
        board.Columns.Add(new Column { Name = "Done" });

        _context.Boards.Add(board);
        await _context.SaveChangesAsync();
        return board;
    }
}