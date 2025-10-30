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
}