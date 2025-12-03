using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskFlow.Api.Controllers;
using TaskFlow.Api.DTOs;
using TaskModel = TaskFlow.Api.Models.Task;
using TaskFlow.Api.Repository.IRepository;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Api.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _mockRepo;
    private readonly TasksController _sut;
    private const int TestUserId = 99;

    public TasksControllerTests()
    {
        _mockRepo = new Mock<ITaskRepository>();
        _sut = new TasksController(_mockRepo.Object);

        // Fake authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString())
        }, "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    // ------------------------------------------------------------
    // MoveTask
    // ------------------------------------------------------------
    [Fact]
    public async Task MoveTask_WhenTaskNotOwnedOrNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new TaskMoveDto { NewColumnId = 50 };

        _mockRepo.Setup(r =>
                r.UpdateTaskColumnAsync(1, dto.NewColumnId, TestUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.MoveTask(1, dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();

        _mockRepo.Verify(r =>
            r.UpdateTaskColumnAsync(1, dto.NewColumnId, TestUserId), Times.Once);
    }

    [Fact]
    public async Task MoveTask_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        var dto = new TaskMoveDto { NewColumnId = 10 };

        _mockRepo.Setup(r =>
                r.UpdateTaskColumnAsync(1, dto.NewColumnId, TestUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.MoveTask(1, dto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // ------------------------------------------------------------
    // ReorderTasks
    // ------------------------------------------------------------
    [Fact]
    public async Task ReorderTasks_WhenInvalid_ReturnsNotFound()
    {
        // Arrange
        var reorderDto = new[]
        {
            new TaskReorderDto { TaskId = 1, NewPosition = 0 }
        };

        _mockRepo.Setup(r =>
                r.UpdateTaskPositionsAsync(1, reorderDto, TestUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ReorderTasks(1, reorderDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ReorderTasks_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        var reorderDto = new[]
        {
            new TaskReorderDto { TaskId = 1, NewPosition = 2 }
        };

        _mockRepo.Setup(r =>
                r.UpdateTaskPositionsAsync(1, reorderDto, TestUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ReorderTasks(1, reorderDto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // ------------------------------------------------------------
    // CreateTask
    // ------------------------------------------------------------
    [Fact]
    public async Task CreateTask_WhenUserNotOwner_ShouldReturnForbid()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "New Task",
            Description = "desc",
            ColumnId = 10
        };

        _mockRepo.Setup(r =>
                r.CreateTaskAsync(It.IsAny<TaskModel>(), TestUserId))
            .ReturnsAsync((TaskModel?)null);

        // Act
        var result = await _sut.CreateTask(dto);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task CreateTask_WhenSuccessful_ShouldReturnOkWithTask()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "My Task",
            Description = "desc",
            ColumnId = 10
        };

        var createdTask = new TaskModel
        {
            Id = 5,
            Title = "My Task",
            Description = "desc",
            ColumnId = 10
        };

        _mockRepo.Setup(r =>
                r.CreateTaskAsync(It.IsAny<TaskModel>(), TestUserId))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _sut.CreateTask(dto);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(createdTask);
    }

    // ------------------------------------------------------------
    // UpdateTask
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateTask_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new TaskUpdateDto { Title = "Updated", Description = "d" };

        _mockRepo.Setup(r =>
                r.UpdateTaskAsync(1, dto, TestUserId))
            .ReturnsAsync((TaskModel?)null);

        // Act
        var result = await _sut.UpdateTask(1, dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateTask_WhenSuccessful_ShouldReturnOkWithTask()
    {
        // Arrange
        var dto = new TaskUpdateDto { Title = "Updated", Description = "desc" };

        var updatedTask = new TaskModel
        {
            Id = 1,
            Title = "Updated",
            Description = "desc"
        };

        _mockRepo.Setup(r =>
                r.UpdateTaskAsync(1, dto, TestUserId))
            .ReturnsAsync(updatedTask);

        // Act
        var result = await _sut.UpdateTask(1, dto);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updatedTask);
    }

    // ------------------------------------------------------------
    // DeleteTask
    // ------------------------------------------------------------
    [Fact]
    public async Task DeleteTask_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.DeleteTaskAsync(1, TestUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteTask(1);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteTask_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        _mockRepo.Setup(r => r.DeleteTaskAsync(1, TestUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteTask(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
