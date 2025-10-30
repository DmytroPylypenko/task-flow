using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Repository.IRepository;

namespace TaskFlow.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ColumnsController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;

    public ColumnsController(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }
    
    /// <summary>
    /// Handles an HTTP PATCH request to reorder tasks within a specific column.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the column whose tasks are being reordered.
    /// </param>
    /// <param name="tasksToReorder">
    /// A collection of task DTOs specifying the task IDs and their new positions.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="UnauthorizedResult"/> if the user's identity cannot be verified.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="NotFoundResult"/> if the column does not exist or the user does not own the tasks.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="NoContentResult"/> if the reorder operation completes successfully.</description>
    ///   </item>
    /// </list>
    /// </returns>
    [HttpPatch("{id}/reorder")]
    public async Task<IActionResult> ReorderTasks(int id, [FromBody] IEnumerable<TaskReorderDto> tasksToReorder)
    {
        // 1. Extract the authenticated user's ID from JWT claims.
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        // 2. Attempt to change the position of the task
        var wasSuccessful = await _taskRepository.UpdateTaskPositionsAsync(id, tasksToReorder, parsedUserId);

        // 3. Return 404 if the column doesn't exist or the user doesn't own the tasks.
        if (!wasSuccessful)
        {
            return NotFound("Column not found or you do not have permission to reorder these tasks.");
        }

        return NoContent(); 
    }
}