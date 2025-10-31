using TaskFlow.Api.DTOs;
using Task = TaskFlow.Api.Models.Task;

namespace TaskFlow.Api.Repository.IRepository;

/// <summary>
/// Defines data access methods for the Task entity.
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Updates the column of a specified task, ensuring that the authenticated user
    /// has ownership of the associated board before performing the operation.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task to move.</param>
    /// <param name="newColumnId">The identifier of the destination column.</param>
    /// <param name="userId">The ID of the authenticated user attempting the update.</param>
    /// <returns>
    /// <c>true</c> if the task was successfully moved; otherwise, <c>false</c>.
    /// Returns <c>false</c> when the task does not exist or the user is unauthorized.
    /// </returns>
    Task<bool> UpdateTaskColumnAsync(int taskId, int newColumnId, int userId);
    
    /// <summary>
    /// Updates the positions of tasks within a specified column, ensuring that all
    /// affected tasks belong to the authenticated user. This method is typically used
    /// when tasks are reordered via drag-and-drop in the UI.
    /// </summary>
    /// <param name="columnId">The ID of the column containing the tasks to reorder.</param>
    /// <param name="tasksToReorder">
    /// A collection of task DTOs containing the task IDs and their new positions.
    /// </param>
    /// <param name="userId">The ID of the authenticated user performing the operation.</param>
    /// <returns>
    /// <c>true</c> if all tasks were successfully updated; otherwise, <c>false</c>
    /// (e.g., if any task does not belong to the specified column or user).
    /// </returns>
    Task<bool> UpdateTaskPositionsAsync(int columnId, IEnumerable<TaskReorderDto> tasksToReorder, int userId);
    
    /// <inheritdoc />
    /// <summary>
    /// Creates a new task in the specified column for the authenticated user.
    /// Ensures that the user owns the column's parent board before inserting
    /// and automatically assigns the correct task position within that column.
    /// </summary>
    /// <param name="task">The task entity to create.</param>
    /// <param name="userId">The ID of the authenticated user creating the task.</param>
    /// <returns>
    /// The newly created <see cref="Task"/> entity, or <c>null</c> if the user
    /// does not have permission to add a task to the specified column.
    /// </returns>
    Task<Task?> CreateTaskAsync(Task task, int userId);
}