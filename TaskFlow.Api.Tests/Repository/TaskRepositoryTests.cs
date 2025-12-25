using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository;
using TaskFlow.Api.Tests.Data;
using Task = System.Threading.Tasks.Task;
using TaskModel = TaskFlow.Api.Models.Task;

namespace TaskFlow.Api.Tests.Repository;

public class TaskRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly TaskRepository _sut;

    public TaskRepositoryTests()
    {
        // Create a new, clean in-memory database for each test
        _context = DbContextFactory.Create();
        _sut = new TaskRepository(_context);
    }

    // -----------------------------
    // Helpers
    // -----------------------------
    private async Task<Column> CreateColumnAsync(int boardUserId)
    {
        var board = new Board { Name = "Board", UserId = boardUserId };
        var column = new Column { Name = "Column", Board = board };

        await _context.Boards.AddAsync(board);
        await _context.Columns.AddAsync(column);
        await _context.SaveChangesAsync();

        return column;
    }

    private async Task<TaskModel> CreateTaskAsync(int columnId, int position)
    {
        var task = new TaskModel
        {
            Title = "Task",
            Description = "Desc",
            ColumnId = columnId,
            Position = position
        };

        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }

    // ------------------------------------------------------------
    // UpdateTaskColumnAsync
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateTaskColumnAsync_WhenUserOwnsBoard_ShouldMoveTask()
    {
        // Arrange
        var userId = 100;
        var col1 = await CreateColumnAsync(userId);
        var col2 = await CreateColumnAsync(userId);
        var task = await CreateTaskAsync(col1.Id, 0);

        // Act
        var result = await _sut.UpdateTaskColumnAsync(task.Id, col2.Id, userId);

        // Assert
        result.Should().BeTrue();

        var taskInDb = await _context.Tasks.FindAsync(task.Id);
        taskInDb!.ColumnId.Should().Be(col2.Id);
    }

    [Fact]
    public async Task UpdateTaskColumnAsync_WhenUserDoesNotOwnBoard_ShouldReturnFalse()
    {
        // Arrange
        var ownerId = 100;
        var attackerId = 999;

        var col = await CreateColumnAsync(ownerId);
        var otherCol = await CreateColumnAsync(ownerId);
        var task = await CreateTaskAsync(col.Id, 0);

        // Act
        var result = await _sut.UpdateTaskColumnAsync(task.Id, otherCol.Id, attackerId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTaskColumnAsync_WhenTaskDoesNotExist_ShouldReturnFalse()
    {
        var result = await _sut.UpdateTaskColumnAsync(999, 1, 100);
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------
    // UpdateTaskPositionsAsync
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateTaskPositionsAsync_WhenAllTasksBelongToUser_ShouldReorderTasks()
    {
        // Arrange
        var userId = 100;
        var col = await CreateColumnAsync(userId);

        var t1 = await CreateTaskAsync(col.Id, 0);
        var t2 = await CreateTaskAsync(col.Id, 1);

        var reorder = new[]
        {
            new TaskReorderDto { TaskId = t1.Id, NewPosition = 1 },
            new TaskReorderDto { TaskId = t2.Id, NewPosition = 0 }
        };

        // Act
        var result = await _sut.UpdateTaskPositionsAsync(col.Id, reorder, userId);

        // Assert
        result.Should().BeTrue();

        var tasks = await _context.Tasks
            .Where(t => t.ColumnId == col.Id)
            .OrderBy(t => t.Position)
            .ToListAsync();

        tasks[0].Id.Should().Be(t2.Id);
        tasks[1].Id.Should().Be(t1.Id);
    }

    [Fact]
    public async Task UpdateTaskPositionsAsync_WhenTasksContainUnauthorizedTask_ShouldReturnFalse()
    {
        // Arrange
        var ownerId = 100;
        var otherId = 900;

        var colOwner = await CreateColumnAsync(ownerId);
        var colOther = await CreateColumnAsync(otherId);

        var t1 = await CreateTaskAsync(colOwner.Id, 0);
        var t2 = await CreateTaskAsync(colOther.Id, 0); // belongs to another user

        var reorder = new[]
        {
            new TaskReorderDto { TaskId = t1.Id, NewPosition = 1 },
            new TaskReorderDto { TaskId = t2.Id, NewPosition = 0 }
        };

        // Act
        var result = await _sut.UpdateTaskPositionsAsync(colOwner.Id, reorder, ownerId);

        // Assert
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------
    // CreateTaskAsync
    // ------------------------------------------------------------
    [Fact]
    public async Task CreateTaskAsync_WhenUserOwnsColumn_ShouldCreateTaskWithCorrectPosition()
    {
        // Arrange
        var userId = 100;
        var col = await CreateColumnAsync(userId);
        await CreateTaskAsync(col.Id, 0);

        var newTask = new TaskModel
        {
            Title = "New Task",
            ColumnId = col.Id
        };

        // Act
        var result = await _sut.CreateTaskAsync(newTask, userId);

        // Assert
        result.Should().NotBeNull();
        result.Position.Should().Be(1); // placed last
    }

    [Fact]
    public async Task CreateTaskAsync_WhenUserDoesNotOwnColumn_ShouldReturnNull()
    {
        // Arrange
        var ownerId = 100;
        var attackerId = 999;

        var col = await CreateColumnAsync(ownerId);

        var task = new TaskModel
        {
            Title = "Hack",
            ColumnId = col.Id
        };

        // Act
        var result = await _sut.CreateTaskAsync(task, attackerId);

        // Assert
        result.Should().BeNull();
    }

    // ------------------------------------------------------------
    // UpdateTaskAsync
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateTaskAsync_WhenUserOwnsTask_ShouldUpdateTaskFields()
    {
        // Arrange
        var userId = 100;
        var col = await CreateColumnAsync(userId);
        var task = await CreateTaskAsync(col.Id, 0);

        var updateDto = new TaskUpdateDto
        {
            Title = "Updated Title",
            Description = "Updated Desc"
        };

        // Act
        var result = await _sut.UpdateTaskAsync(task.Id, updateDto, userId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Desc");
    }

    [Fact]
    public async Task UpdateTaskAsync_WhenUserDoesNotOwnTask_ShouldReturnNull()
    {
        // Arrange
        var ownerId = 100;
        var attackerId = 999;

        var col = await CreateColumnAsync(ownerId);
        var task = await CreateTaskAsync(col.Id, 0);

        var dto = new TaskUpdateDto { Title = "Hack", Description = "Hack" };

        // Act
        var result = await _sut.UpdateTaskAsync(task.Id, dto, attackerId);

        // Assert
        result.Should().BeNull();
    }

    // ------------------------------------------------------------
    // DeleteTaskAsync
    // ------------------------------------------------------------
    [Fact]
    public async Task DeleteTaskAsync_WhenUserOwnsTask_ShouldDeleteTask()
    {
        // Arrange
        var userId = 100;
        var col = await CreateColumnAsync(userId);
        var task = await CreateTaskAsync(col.Id, 0);

        // Act
        var result = await _sut.DeleteTaskAsync(task.Id, userId);

        // Assert
        result.Should().BeTrue();
        (await _context.Tasks.FindAsync(task.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenUserDoesNotOwnTask_ShouldReturnFalse()
    {
        // Arrange
        var ownerId = 100;
        var attackerId = 999;

        var col = await CreateColumnAsync(ownerId);
        var task = await CreateTaskAsync(col.Id, 0);

        // Act
        var result = await _sut.DeleteTaskAsync(task.Id, attackerId);

        // Assert
        result.Should().BeFalse();
    }
}