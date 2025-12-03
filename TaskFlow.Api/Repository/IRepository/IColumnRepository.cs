using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Repository.IRepository;

public interface IColumnRepository
{
    /// <summary>
    /// Creates a new column within a board owned by the specified user.
    /// </summary>
    /// <param name="column">The column entity to create.</param>
    /// <param name="userId">
    /// The ID of the authenticated user attempting to create the column.  
    /// Ensures that users can only add columns to their own boards.
    /// </param>
    /// <returns>
    /// The newly created <see cref="Column"/> if the board exists and is owned by the user;  
    /// otherwise, <c>null</c>.
    /// </returns>
    Task<Column?> CreateColumnAsync(Column column, int userId);

    /// <summary>
    /// Updates the name of an existing column owned by the authenticated user.
    /// </summary>
    /// <param name="columnId">The ID of the column to update.</param>
    /// <param name="columnDto">The data containing the new column name.</param>
    /// <param name="userId">
    /// The ID of the user attempting the update.  
    /// Ensures that a user can only modify their own columns.
    /// </param>
    /// <returns>
    /// The updated <see cref="Column"/> if the column exists and is owned by the user;  
    /// otherwise, <c>null</c>.
    /// </returns>
    Task<Column?> UpdateColumnAsync(int columnId, ColumnUpdateDto columnDto, int userId);

    /// <summary>
    /// Deletes a column belonging to a board owned by the authenticated user.
    /// </summary>
    /// <param name="columnId">The ID of the column to delete.</param>
    /// <param name="userId">
    /// The ID of the user performing the deletion.  
    /// Used to validate ownership of the board and its columns.
    /// </param>
    /// <returns>
    /// <c>true</c> if the column existed and was successfully deleted;  
    /// <c>false</c> if the column does not exist or does not belong to the user.
    /// </returns>
    Task<bool> DeleteColumnAsync(int columnId, int userId);
}