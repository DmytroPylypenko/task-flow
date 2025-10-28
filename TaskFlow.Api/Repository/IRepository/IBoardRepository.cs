using TaskFlow.Api.Models;

namespace TaskFlow.Api.Repository.IRepository;

/// <summary>
/// Defines data access methods for the Board entity.
/// </summary>
public interface IBoardRepository
{
    /// <summary>
    /// Gets all boards for a specific user.
    /// </summary>
    Task<IEnumerable<Board>> GetBoardsByUserIdAsync(int userId);
    
    /// <summary>
    /// Gets a single board by its ID, ensuring it belongs to the specified user.
    /// </summary>
    /// <param name="boardId">The ID of the board to retrieve.</param>
    /// <param name="userId">The ID of the user who must own the board.</param>
    /// <returns>The Board object if found and ownership is verified; otherwise, null.</returns>
    Task<Board?> GetBoardByIdAsync(int boardId, int userId);
}