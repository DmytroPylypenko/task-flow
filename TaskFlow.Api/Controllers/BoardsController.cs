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
}