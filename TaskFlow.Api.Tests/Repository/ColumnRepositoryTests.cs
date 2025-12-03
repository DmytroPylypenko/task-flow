using FluentAssertions;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository;
using TaskFlow.Api.Tests.Data;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Api.Tests.Repository;

public class ColumnRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly ColumnRepository _sut;

    public ColumnRepositoryTests()
    {
        _context = DbContextFactory.Create();
        _sut = new ColumnRepository(_context);
    }

    // -----------------------------
    // Helpers
    // -----------------------------
    private async Task<Board> CreateBoardAsync(int userId)
    {
        var board = new Board
        {
            Name = "Board",
            UserId = userId
        };

        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();
        return board;
    }

    private async Task<Column> CreateColumnAsync(int boardId, string name = "Column")
    {
        var column = new Column
        {
            Name = name,
            BoardId = boardId
        };

        await _context.Columns.AddAsync(column);
        await _context.SaveChangesAsync();
        return column;
    }

    // ------------------------------------------------------------
    // CreateColumnAsync
    // ------------------------------------------------------------
    [Fact]
    public async Task CreateColumnAsync_WhenUserOwnsBoard_ShouldCreateColumn()
    {
        // Arrange
        var userId = 100;
        var board = await CreateBoardAsync(userId);

        var column = new Column
        {
            Name = "New Column",
            BoardId = board.Id
        };

        // Act
        var result = await _sut.CreateColumnAsync(column, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var columnInDb = await _context.Columns.FindAsync(result.Id);
        columnInDb.Should().NotBeNull();
        columnInDb.Name.Should().Be("New Column");
    }

    [Fact]
    public async Task CreateColumnAsync_WhenUserDoesNotOwnBoard_ShouldReturnNull()
    {
        // Arrange
        var ownerId = 100;
        var attackerId = 999;

        var board = await CreateBoardAsync(ownerId);

        var column = new Column
        {
            Name = "Unauthorized",
            BoardId = board.Id
        };

        // Act
        var result = await _sut.CreateColumnAsync(column, attackerId);

        // Assert
        result.Should().BeNull();
    }

    // ------------------------------------------------------------
    // UpdateColumnAsync
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateColumnAsync_WhenUserOwnsColumn_ShouldUpdateColumn()
    {
        // Arrange
        var userId = 100;
        var board = await CreateBoardAsync(userId);
        var column = await CreateColumnAsync(board.Id, "Old Name");

        var updateDto = new ColumnUpdateDto
        {
            Name = "Updated Name"
        };

        // Act
        var result = await _sut.UpdateColumnAsync(column.Id, updateDto, userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateColumnAsync_WhenUserDoesNotOwnBoard_ShouldReturnNull()
    {
        // Arrange
        var ownerId = 100;
        var attackerId = 999;

        var board = await CreateBoardAsync(ownerId);
        var column = await CreateColumnAsync(board.Id);

        var updateDto = new ColumnUpdateDto
        {
            Name = "Hack"
        };

        // Act
        var result = await _sut.UpdateColumnAsync(column.Id, updateDto, attackerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateColumnAsync_WhenColumnDoesNotExist_ShouldReturnNull()
    {
        var updateDto = new ColumnUpdateDto
        {
            Name = "New Name"
        };

        var result = await _sut.UpdateColumnAsync(999, updateDto, 100);

        result.Should().BeNull();
    }

    // ------------------------------------------------------------
    // DeleteColumnAsync
    // ------------------------------------------------------------
    [Fact]
    public async Task DeleteColumnAsync_WhenUserOwnsColumn_ShouldDeleteColumn()
    {
        // Arrange
        var userId = 100;
        var board = await CreateBoardAsync(userId);
        var column = await CreateColumnAsync(board.Id);

        // Act
        var result = await _sut.DeleteColumnAsync(column.Id, userId);

        // Assert
        result.Should().BeTrue();
        (await _context.Columns.FindAsync(column.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteColumnAsync_WhenUserDoesNotOwnBoard_ShouldReturnFalse()
    {
        // Arrange
        var ownerId = 100;
        var attackerId = 999;

        var board = await CreateBoardAsync(ownerId);
        var column = await CreateColumnAsync(board.Id);

        // Act
        var result = await _sut.DeleteColumnAsync(column.Id, attackerId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteColumnAsync_WhenColumnDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var result = await _sut.DeleteColumnAsync(999, 100);

        // Assert
        result.Should().BeFalse();
    }
}