using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskFlow.Api.Controllers;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Api.Tests.Controllers;

public class ColumnsControllerTests
{
    private readonly Mock<IColumnRepository> _mockRepo;
    private readonly ColumnsController _sut;
    private const int TestUserId = 99;

    public ColumnsControllerTests()
    {
        _mockRepo = new Mock<IColumnRepository>();
        _sut = new ColumnsController(_mockRepo.Object);

        // Fake authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString())
        ], "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    // ------------------------------------------------------------
    // CreateColumn
    // ------------------------------------------------------------
    [Fact]
    public async Task CreateColumn_WhenBoardNotOwned_ShouldReturnForbid()
    {
        // Arrange
        var dto = new ColumnCreateDto { Name = "New Column", BoardId = 1 };

        _mockRepo.Setup(r => r.CreateColumnAsync(It.IsAny<Column>(), TestUserId))
            .ReturnsAsync((Column?)null);

        // Act
        var result = await _sut.CreateColumn(dto);

        // Assert
        result.Should().BeOfType<ForbidResult>();

        _mockRepo.Verify(r => r.CreateColumnAsync(It.IsAny<Column>(), TestUserId), Times.Once);
    }

    [Fact]
    public async Task CreateColumn_WhenSuccessful_ShouldReturnOkWithColumn()
    {
        // Arrange
        var dto = new ColumnCreateDto { Name = "To Do", BoardId = 10 };
        var createdColumn = new Column { Id = 5, Name = "To Do", BoardId = 10 };

        _mockRepo.Setup(r => r.CreateColumnAsync(It.IsAny<Column>(), TestUserId))
            .ReturnsAsync(createdColumn);

        // Act
        var result = await _sut.CreateColumn(dto);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(createdColumn);
    }

    // ------------------------------------------------------------
    // UpdateColumn
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateColumn_WhenColumnNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new ColumnUpdateDto { Name = "Updated Name" };

        _mockRepo.Setup(r => r.UpdateColumnAsync(1, dto, TestUserId))
            .ReturnsAsync((Column?)null);

        // Act
        var result = await _sut.UpdateColumn(1, dto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateColumn_WhenSuccessful_ShouldReturnOkWithUpdatedColumn()
    {
        // Arrange
        var dto = new ColumnUpdateDto { Name = "Updated Column" };
        var updatedColumn = new Column { Id = 1, Name = "Updated Column" };

        _mockRepo.Setup(r => r.UpdateColumnAsync(1, dto, TestUserId))
            .ReturnsAsync(updatedColumn);

        // Act
        var result = await _sut.UpdateColumn(1, dto);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updatedColumn);
    }

    // ------------------------------------------------------------
    // DeleteColumn
    // ------------------------------------------------------------
    [Fact]
    public async Task DeleteColumn_WhenColumnNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.DeleteColumnAsync(1, TestUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteColumn(1);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteColumn_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        _mockRepo.Setup(r => r.DeleteColumnAsync(1, TestUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteColumn(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
