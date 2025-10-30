using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Repository.IRepository;
using Task = TaskFlow.Api.Models.Task;
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
    
    /// <inheritdoc />
    public async Task<bool> UpdateTaskPositionsAsync(int columnId, IEnumerable<TaskReorderDto> tasksToReorder, int userId)
    {
        // 1. Retrieve all tasks in the target column that belong to the user.
        var tasksInColumn = await _context.Tasks
            .Where(t => t.ColumnId == columnId && t.Column.Board.UserId == userId)
            .ToListAsync();

        // 2. Verify that all tasks to be reordered exist in this column.
        var taskIdsToReorder = tasksToReorder.Select(t => t.TaskId).ToHashSet();
        if (!taskIdsToReorder.IsSubsetOf(tasksInColumn.Select(t => t.Id)))
        {
            // A task ID was provided that doesn't belong to this column/user.
            return false; 
        }

        // 3. Update the position of each task in memory.
        foreach (TaskReorderDto taskDto in tasksToReorder)
        {
            Task taskToUpdate = tasksInColumn.First(t => t.Id == taskDto.TaskId);
            taskToUpdate.Position = taskDto.NewPosition;
        }

        // 4. Save all changes to the database in a single transaction.
        await _context.SaveChangesAsync();
        return true;
    }
}