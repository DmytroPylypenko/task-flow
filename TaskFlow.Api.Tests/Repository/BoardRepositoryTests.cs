using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository;
using TaskFlow.Api.Tests.Data;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Api.Tests.Repository;

public class BoardRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly BoardRepository _sut;

    public BoardRepositoryTests()
    {
        // Create a new, clean in-memory database for each test
        _context = DbContextFactory.Create();
        _sut = new BoardRepository(_context);
    }

    [Fact]
    public async Task CreateBoardAsync_WhenCalled_ShouldAddBoardAndDefaultColumns()
    {
        // Arrange
        var board = new Board { Name = "New Project", UserId = 100 };

        // Act
        var result = await _sut.CreateBoardAsync(board);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        // Verify the board is in the DB
        var boardInDb = await _context.Boards
            .Include(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == result.Id);

        boardInDb.Should().NotBeNull();
        boardInDb.Name.Should().Be("New Project");

        // Verify default columns were created
        boardInDb.Columns.Should().HaveCount(3);
        boardInDb.Columns.Select(c => c.Name).Should().Contain(new[] { "To Do", "In Progress", "Done" });
    }

    [Fact]
    public async Task GetBoardsByUserIdAsync_WhenUserHasBoards_ShouldReturnOnlyUserBoardsOrdered()
    {
        // Arrange
        var userId = 100;
        var otherUserId = 999;

        await _context.Boards.AddRangeAsync(new[]
        {
            new Board { Id = 1, Name = "Old Board", UserId = userId },
            new Board { Id = 2, Name = "New Board", UserId = userId },
            new Board { Id = 3, Name = "Other User's Board", UserId = otherUserId }
        });
        await _context.SaveChangesAsync();

        // Act
        var result = (await _sut.GetBoardsByUserIdAsync(userId)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.UserId == userId);

        // Check ordering (Newest/Highest ID first)
        result.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task GetBoardsByUserIdAsync_WhenUserHasNoBoards_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetBoardsByUserIdAsync(123);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBoardByIdAsync_WhenUserOwnsBoard_ShouldReturnBoard()
    {
        // Arrange
        var userId = 100;
        var board = new Board { Id = 1, Name = "My Board", UserId = userId };
        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetBoardByIdAsync(board.Id, userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("My Board");
    }

    [Fact]
    public async Task GetBoardByIdAsync_WhenUserDoesNotOwnBoard_ShouldReturnNull()
    {
        // Arrange
        var userId = 100;
        var otherUserId = 999;
        var board = new Board { Name = "Secret Board", UserId = otherUserId };
        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetBoardByIdAsync(board.Id, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateBoardAsync_WhenUserOwnsBoard_ShouldUpdateName()
    {
        // Arrange
        var userId = 100;
        var board = new Board { Id = 1, Name = "Old Name", UserId = userId };
        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.UpdateBoardAsync(board.Id, "New Name", userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");

        var dbBoard = await _context.Boards.FindAsync(1);
        dbBoard!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateBoardAsync_WhenBoardNotFound_ShouldReturnNull()
    {
        // Act
        var result = await _sut.UpdateBoardAsync(123, "Name", 10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteBoardAsync_WhenUserOwnsBoard_ShouldRemoveBoard()
    {
        // Arrange
        var userId = 100;
        var board = new Board { Id = 1, Name = "To Delete", UserId = userId };
        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.DeleteBoardAsync(board.Id, userId);

        // Assert
        result.Should().BeTrue();
        (await _context.Boards.FindAsync(1)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteBoardAsync_WhenUserDoesNotOwnBoard_ShouldReturnFalse()
    {
        // Arrange
        var userId = 100;
        var otherUserId = 999;
        var board = new Board { Id = 1, Name = "Safe Board", UserId = otherUserId };
        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.DeleteBoardAsync(board.Id, userId);

        // Assert
        result.Should().BeFalse();
        (await _context.Boards.FindAsync(1)).Should().NotBeNull();
    }
}