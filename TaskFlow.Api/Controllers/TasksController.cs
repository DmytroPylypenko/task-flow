using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Repository.IRepository;
using Task =  TaskFlow.Api.Models.Task;

namespace TaskFlow.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;

    public TasksController(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }
    
    /// <summary>
    /// Handles an HTTP PATCH request to move an existing task
    /// to a different column within a board owned by the authenticated user.
    /// </summary>
    /// <param name="id">The unique identifier of the task to move.</param>
    /// <param name="taskMoveDto">The data transfer object containing the target column ID.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the operation:
    /// <list type="bullet">
    ///   <item><description><see cref="UnauthorizedResult"/> if the user ID cannot be determined.</description></item>
    ///   <item><description><see cref="NotFoundResult"/> if the task does not exist or is not owned by the user.</description></item>
    ///   <item><description><see cref="NoContentResult"/> if the operation succeeds.</description></item>
    /// </list>
    /// </returns>
    [HttpPatch("{id}/move")]
    public async Task<IActionResult> MoveTask(int id, [FromBody] TaskMoveDto taskMoveDto)
    {
        // 1. Extract the authenticated user's ID from JWT claims.
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        // 2. Attempt to move the task to the specified column.
        var wasSuccessful = await _taskRepository.UpdateTaskColumnAsync(id, taskMoveDto.NewColumnId, parsedUserId);

        // 3. Return 404 if the task does not exist or is not owned by the user.
        if (!wasSuccessful)
        {
            return NotFound("Task not found or you do not have permission to move it.");
        }
        
        return NoContent();
    }
    
    /// <summary>
    /// Handles HTTP POST requests to create a new task within a specific column.
    /// </summary>
    /// <param name="taskDto">
    /// The data transfer object containing the new task's title, description, and column ID.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the outcome of the operation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="UnauthorizedResult"/> if the user's identity cannot be verified.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="ForbidResult"/> if the user does not own the target column.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="OkObjectResult"/> containing the newly created task if successful.</description>
    ///   </item>
    /// </list>
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto taskDto)
    {
        // 1. Extract the authenticated user's ID from JWT claims.
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        // 2. Map the DTO to the Task model
        var taskToCreate = new Task
        {
            Title = taskDto.Title,
            Description = taskDto.Description,
            ColumnId = taskDto.ColumnId
        };

        // 3. Use the repository to create the task.
        var createdTask = await _taskRepository.CreateTaskAsync(taskToCreate, parsedUserId);

        // 4. Handle forbidden action 403 — user tries to add a task to a column they don’t own.
        if (createdTask == null)
        {
            return Forbid(); 
        }
        
        return Ok(createdTask);
    }
}