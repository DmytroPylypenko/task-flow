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
            .ThenInclude(c => c!.Board)
            .FirstOrDefaultAsync(t =>
                t.Id == taskId && t.Column != null && t.Column.Board != null && t.Column.Board.UserId == userId);

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
    public async Task<bool> UpdateTaskPositionsAsync(int columnId, IEnumerable<TaskReorderDto> tasksToReorder,
        int userId)
    {
        // 1. Retrieve all tasks in the target column that belong to the user.
        var tasksInColumn = await _context.Tasks
            .Where(t => t.ColumnId == columnId && t.Column != null && t.Column.Board != null &&
                        t.Column.Board.UserId == userId)
            .ToListAsync();

        // 2. Verify that all tasks to be reordered exist in this column.
        var reorderList = tasksToReorder.ToList();
        var taskIdsToReorder = reorderList.Select(t => t.TaskId).ToHashSet();
        if (!taskIdsToReorder.IsSubsetOf(tasksInColumn.Select(t => t.Id)))
        {
            // A task ID was provided that doesn't belong to this column/user.
            return false;
        }

        // 3. Update the position of each task in memory.
        foreach (TaskReorderDto taskDto in reorderList)
        {
            Task taskToUpdate = tasksInColumn.First(t => t.Id == taskDto.TaskId);
            taskToUpdate.Position = taskDto.NewPosition;
        }

        // 4. Save all changes to the database in a single transaction.
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<Task?> CreateTaskAsync(Task task, int userId)
    {
        // 1. Security Check: Verify the user owns the board the column belongs to.
        var column = await _context.Columns
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == task.ColumnId && c.Board != null && c.Board.UserId == userId);

        // If the user does not own the column, deny the operation.
        if (column == null)
        {
            return null;
        }

        // 2. Determine the new task's position (it should appear last).
        var maxPosition = await _context.Tasks
            .Where(t => t.ColumnId == task.ColumnId)
            .DefaultIfEmpty()
            .MaxAsync(t => (int?)t!.Position);

        // If no tasks exist, start from 0; otherwise, increment by 1.
        task.Position = (maxPosition ?? -1) + 1;

        // 3. Add and save the new task.
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    /// <inheritdoc />
    public async Task<Task?> UpdateTaskAsync(int taskId, TaskUpdateDto taskDto, int userId)
    {
        // 1. Find the task and verify the user owns the board it belongs to.
        var task = await _context.Tasks
            .Include(t => t.Column)
            .ThenInclude(c => c!.Board)
            .FirstOrDefaultAsync(t =>
                t.Id == taskId && t.Column != null && t.Column.Board != null && t.Column.Board.UserId == userId);

        // If the task is not found or the user does not own the board, return null
        if (task == null)
        {
            return null;
        }

        // 2. Apply updates from the DTO to the entity.
        task.Title = taskDto.Title;
        task.Description = taskDto.Description;

        await _context.SaveChangesAsync();
        return task;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteTaskAsync(int taskId, int userId)
    {
        // 1. Find the task and verify the user owns the board it belongs to.
        var task = await _context.Tasks
            .Include(t => t.Column)
            .ThenInclude(c => c!.Board)
            .FirstOrDefaultAsync(t =>
                t.Id == taskId && t.Column != null && t.Column.Board != null && t.Column.Board.UserId == userId);

        // If the task is not found or the user does not own the board, return null
        if (task == null)
        {
            return false;
        }

        // 2. Remove the task and save changes.
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}