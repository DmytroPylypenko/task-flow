using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Repository.IRepository;

namespace TaskFlow.Api.Repository;

/// <summary>
/// Implements data access for the Task entity using Entity Framework Core.
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateTaskColumnAsync(int taskId, int newColumnId, int userId)
    {
        // Find the task and verify the user owns the board it belongs to.
        // This is a critical security check to prevent users from moving tasks they don't own.
        var task = await _context.Tasks
            .Include(t => t.Column)
            .ThenInclude(c => c.Board)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.Column.Board.UserId == userId);

        // Task not found.
        if (task == null)
        {
            return false;
        }

        // Update the task's column ID and save changes.
        task.ColumnId = newColumnId;
        await _context.SaveChangesAsync();
        return true;
    }
}