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
    
    /// <summary>
    /// Handles HTTP PUT requests to update an existing task's title and description.
    /// Ensures that the authenticated user owns the board the task belongs to
    /// before applying updates.
    /// </summary>
    /// <param name="id">The unique identifier of the task to update.</param>
    /// <param name="taskDto">
    /// The data transfer object containing the updated title and description fields.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the result of the operation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="UnauthorizedResult"/> if the user cannot be identified from the JWT claims.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="NotFoundResult"/> if the task does not exist or does not belong to the authenticated user.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="OkObjectResult"/> containing the updated task if the operation succeeds.</description>
    ///   </item>
    /// </list>
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdateDto taskDto)
    {
        // 1. Extract the authenticated user's ID from JWT claims.
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        // 2. Use the repository to update the task.
        var updatedTask = await _taskRepository.UpdateTaskAsync(id, taskDto, parsedUserId);

        // 3. Return 404 if the task does not exist or is not owned by the user.
        if (updatedTask == null)
        {
            return NotFound("Task not found or you do not have permission to edit it.");
        }

        return Ok(updatedTask);
    }

    /// <summary>
    /// Handles HTTP DELETE requests to remove a specific task from the database.
    /// Ensures that only the authenticated user who owns the board can delete the task.
    /// </summary>
    /// <param name="id">The unique identifier of the task to delete.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the result of the operation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="UnauthorizedResult"/> if the user's identity cannot be verified.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="NotFoundResult"/> if the task does not exist or does not belong to the authenticated user.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="NoContentResult"/> if the task was successfully deleted.</description>
    ///   </item>
    /// </list>
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        // 1. Extract the authenticated user's ID from JWT claims.
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        // 2. Attempt to delete the task.
        var wasSuccessful = await _taskRepository.DeleteTaskAsync(id, parsedUserId);

        // 3. Return 404 if the task does not exist or is not owned by the user.
        if (!wasSuccessful)
        {
            return NotFound("Task not found or you do not have permission to delete it.");
        }

        // 4. Return 204 No Content to indicate the task was successfully deleted.
        return NoContent();
    }
}