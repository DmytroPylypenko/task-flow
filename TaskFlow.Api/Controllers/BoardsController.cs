using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;

namespace TaskFlow.Api.Controllers;

/// <summary>
/// Exposes secure API endpoints for managing user boards.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly IBoardRepository  _boardRepository;

    public BoardsController(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    /// <summary>
    /// Gets all boards for the currently authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBoards()
    {
        // 1. Get user ID from claims
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        // 2. Safely parse user ID
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return BadRequest("Invalid user ID.");
        }
        
        // 3. Get boards from repository
        var boards = await _boardRepository.GetBoardsByUserIdAsync(parsedUserId);
        
        return Ok(boards);
    }
    
    /// <summary>
    /// Gets a single board by its ID, including its columns and tasks.
    /// </summary>
    /// <param name="id">The ID of the board to retrieve.</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBoard(int id)
    {
        // 1. Get user ID from claims
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // 2. Safely parse user ID
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }
        
        // 3. Get a board from repository
        var board = await _boardRepository.GetBoardByIdAsync(id, parsedUserId);

        // 4. Handle not found or unauthorized access.
        if (board == null)
        {
            return NotFound();
        }

        return Ok(board);
    }
    
    /// <summary>
    /// Creates a new board for the authenticated user.
    /// </summary>
    /// <param name="boardDto">The data for the new board.</param>
    /// <returns>The newly created board.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] BoardCreateDto boardDto)
    {
        // 1. Get user ID from claims
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // 2. Safely parse user ID
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        // 3. Create the Board model from the DTO.
        var board = new Board
        {
            Name = boardDto.Name,
            UserId = parsedUserId
        };

        // 4. Use the repository to create the board.
        var createdBoard = await _boardRepository.CreateBoardAsync(board);

        // 5. Return a 201 Created response with a Location header.
        return CreatedAtAction(nameof(GetBoard), new { id = createdBoard.Id }, createdBoard);
    }
    
    /// <summary>
    /// Handles HTTP PUT requests to update the name of an existing board owned by the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the board to update.</param>
    /// <param name="boardDto">The request payload containing the updated board name.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> containing the updated board if the operation succeeds;  
    /// <see cref="NotFoundObjectResult"/> if the board does not exist or is not owned by the user;  
    /// <see cref="UnauthorizedResult"/> if the user's identity cannot be verified.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBoard(int id, [FromBody] BoardUpdateDto boardDto)
    {
        // 1. Extract the authenticated user's ID from JWT claims.
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        // 2. Attempt to update the board.
        var updatedBoard = await _boardRepository.UpdateBoardAsync(id, boardDto.Name, parsedUserId);

        // 3. Return 404 if the task does not exist or is not owned by the user.
        if (updatedBoard == null)
        {
            return NotFound("Board not found or you do not have permission to edit it.");
        }

        return Ok(updatedBoard);
    }
    
    /// <summary>
    /// Deletes a board owned by the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the board to delete.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> if the board was successfully deleted;  
    /// <see cref="NotFoundObjectResult"/> if the board does not exist or the user is not authorized;  
    /// <see cref="UnauthorizedResult"/> if the user's identity could not be verified.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(int id)
    {
        // 1. Extract the authenticated user's ID from JWT claims.
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        // 2. Attempt to delete the board.
        var wasSuccessful = await _boardRepository.DeleteBoardAsync(id, parsedUserId);

        // 3. Return 404 if the task does not exist or is not owned by the user.
        if (!wasSuccessful)
        {
            return NotFound("Board not found or you do not have permission to delete it.");
        }

        return NoContent(); 
    }
}