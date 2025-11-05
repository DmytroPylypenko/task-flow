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
    
    /// <summary>
    /// Adds a new board to the database for the specified user.
    /// Automatically creates the default columns (To Do, In Progress, Done).
    /// </summary>
    /// <param name="board">The Board object to create.</param>
    /// <returns>The newly created Board object, including its generated ID.</returns>
    Task<Board> CreateBoardAsync(Board board);
    
    /// <summary>
    /// Deletes a board owned by the specified user.
    /// </summary>
    /// <param name="boardId">The ID of the board to delete.</param>
    /// <param name="userId">
    /// The ID of the authenticated user attempting the deletion.  
    /// Used to verify that the user can only delete their own boards.
    /// </param>
    /// <returns>
    /// <c>true</c> if the board existed and was successfully deleted;  
    /// <c>false</c> if the board was not found or did not belong to the user.
    /// </returns>
    Task<bool> DeleteBoardAsync(int boardId, int userId);
}