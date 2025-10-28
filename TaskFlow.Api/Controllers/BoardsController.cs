using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}