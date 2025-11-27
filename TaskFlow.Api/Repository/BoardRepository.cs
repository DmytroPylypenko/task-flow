using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;

namespace TaskFlow.Api.Repository;

/// <summary>
/// Implements data access for the Board entity using Entity Framework Core.
/// </summary>
public class BoardRepository : IBoardRepository
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
            .Include(b => b.Columns)
            .OrderByDescending(b => b.Id)
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

    /// <inheritdoc />
    public async Task<Board?> UpdateBoardAsync(int boardId, string newName, int userId)
    {
        // 1. Find the board and verify ownership.
        var board = await _context.Boards
            .FirstOrDefaultAsync(b => b.Id == boardId && b.UserId == userId);

        if (board == null)
        {
            return null;
        }

        // 2. Rename the board
        board.Name = newName;
        await _context.SaveChangesAsync();
        return board;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteBoardAsync(int boardId, int userId)
    {
        // 1. Find the board and verify ownership.
        var board = await _context.Boards
            .FirstOrDefaultAsync(b => b.Id == boardId && b.UserId == userId);

        if (board == null)
        {
            return false;
        }

        // 2. Remove the board.
        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();
        return true;
    }
}