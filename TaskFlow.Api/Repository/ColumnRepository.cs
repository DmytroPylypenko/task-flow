using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;

namespace TaskFlow.Api.Repository;

public class ColumnRepository : IColumnRepository
{
    private readonly ApplicationDbContext _context;

    public ColumnRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Column?> CreateColumnAsync(Column column, int userId)
    {
        // 1. Verify that the user owns the board the column belongs to.
        var boardExists = await _context.Boards.AnyAsync(b => b.Id == column.BoardId && b.UserId == userId);
        if (!boardExists) return null;

        // 2. Add the column and save changes.
        _context.Columns.Add(column);
        await _context.SaveChangesAsync();
        return column;
    }

    /// <inheritdoc />
    public async Task<Column?> UpdateColumnAsync(int columnId, ColumnUpdateDto columnDto, int userId)
    {
        // 1. Retrieve the column and ensure the user owns the board it belongs to.
        var column = await _context.Columns
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId && c.Board.UserId == userId);

        if (column == null) return null;

        // 2. Update the column name and persist changes.
        column.Name = columnDto.Name;
        await _context.SaveChangesAsync();
        return column;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteColumnAsync(int columnId, int userId)
    {
        // 1. Retrieve the column and ensure the user owns the board it belongs to.
        var column = await _context.Columns
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId && c.Board.UserId == userId);

        if (column == null) return false;

        // 2. Delete the column and commit changes.
        _context.Columns.Remove(column);
        await _context.SaveChangesAsync();
        return true;
    }
}