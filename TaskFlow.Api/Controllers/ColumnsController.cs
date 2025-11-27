using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;

namespace TaskFlow.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ColumnsController : ControllerBase
{
    private readonly IColumnRepository _columnRepository;

    public ColumnsController(IColumnRepository columnRepository)
    {
        _columnRepository = columnRepository;
    }

    /// <summary>
    /// Handles an HTTP POST request to create a new column within a board owned by the authenticated user.
    /// </summary>
    /// <param name="columnDto">
    /// The data required to create the column, including the column name and the target board ID.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> describing the result of the operation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="UnauthorizedResult"/> if the user's identity cannot be verified.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="ForbidResult"/> if the board does not belong to the authenticated user.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="OkObjectResult"/> containing the newly created column when successful.</description>
    ///   </item>
    /// </list>
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateColumn([FromBody] ColumnCreateDto columnDto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId)) return Unauthorized();

        var columnToCreate = new Column { Name = columnDto.Name, BoardId = columnDto.BoardId };
        var createdColumn = await _columnRepository.CreateColumnAsync(columnToCreate, userId);

        if (createdColumn == null) return Forbid();

        return Ok(createdColumn);
    }

    /// <summary>
    /// Handles an HTTP PUT request to update the name of an existing column owned by the authenticated user.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the column to be updated.
    /// </param>
    /// <param name="columnDto">
    /// The DTO containing the updated column name.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> describing the outcome of the request:
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="UnauthorizedResult"/> if the user's identity cannot be validated.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="NotFoundResult"/> if the column does not exist or does not belong to the user.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="OkObjectResult"/> containing the updated column upon successful update.</description>
    ///   </item>
    /// </list>
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateColumn(int id, [FromBody] ColumnUpdateDto columnDto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId)) return Unauthorized();

        var updatedColumn = await _columnRepository.UpdateColumnAsync(id, columnDto, userId);

        if (updatedColumn == null) return NotFound();

        return Ok(updatedColumn);
    }

    /// <summary>
    /// Handles an HTTP DELETE request to remove a column and all tasks within it, 
    /// provided the column belongs to the authenticated user.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the column to delete.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> describing the result:
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="UnauthorizedResult"/> if user authentication fails.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="NotFoundResult"/> if the column does not exist or the user does not own it.</description>
    ///   </item>
    ///   <item>
    ///     <description><see cref="NoContentResult"/> when the deletion completes successfully.</description>
    ///   </item>
    /// </list>
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteColumn(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId)) return Unauthorized();

        var wasSuccessful = await _columnRepository.DeleteColumnAsync(id, userId);

        if (!wasSuccessful) return NotFound();

        return NoContent();
    }
}