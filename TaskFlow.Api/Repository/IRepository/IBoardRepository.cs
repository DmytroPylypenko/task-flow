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
}