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

public class BoardsControllerTests
{
    private readonly Mock<IBoardRepository> _mockRepo;
    private readonly BoardsController _sut;

    private const int TestUserId = 99;

    public BoardsControllerTests()
    {
        _mockRepo = new Mock<IBoardRepository>();
        _sut = new BoardsController(_mockRepo.Object);

        // Simulate logged in user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, TestUserId.ToString())
        }, "mock"));

        _sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    // ------------------------------------------------------------
    // GetBoards
    // ------------------------------------------------------------
    [Fact]
    public async Task GetBoards_WhenBoardsExist_ShouldReturnOkWithBoards()
    {
        // Arrange
        var boards = new List<Board> { new(), new() };
        _mockRepo.Setup(repo => repo.GetBoardsByUserIdAsync(TestUserId))
            .ReturnsAsync(boards);

        // Act
        var result = await _sut.GetBoards();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<Board>>();
        okResult.Value.As<IEnumerable<Board>>().Should().HaveCount(2);

        _mockRepo.Verify(r => r.GetBoardsByUserIdAsync(TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetBoards_WhenUserIdMissing_ShouldReturnUnauthorized()
    {
        // Arrange
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal();

        // Act
        var result = await _sut.GetBoards();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    // ------------------------------------------------------------
    // GetBoard
    // ------------------------------------------------------------
    [Fact]
    public async Task GetBoard_WhenBoardExists_ShouldReturnOk()
    {
        // Arrange
        var boardId = 1;
        var board = new Board { Id = boardId, Name = "Test Board" };
        _mockRepo.Setup(repo => repo.GetBoardByIdAsync(boardId, TestUserId))
            .ReturnsAsync(board);

        // Act
        var result = await _sut.GetBoard(boardId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(board);

        _mockRepo.Verify(r => r.GetBoardByIdAsync(1, TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetBoard_WhenBoardDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var boardId = 1;
        _mockRepo.Setup(repo => repo.GetBoardByIdAsync(boardId, TestUserId))
            .ReturnsAsync((Board?)null);

        // Act
        var result = await _sut.GetBoard(boardId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetBoard_WhenInvalidUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var invalidUser = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "abc")]));

        _sut.ControllerContext.HttpContext.User = invalidUser;

        // Act
        var result = await _sut.GetBoard(1);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    // ------------------------------------------------------------
    // CreateBoard
    // ------------------------------------------------------------
    [Fact]
    public async Task CreateBoard_WhenSuccessful_ShouldReturnCreated()
    {
        // Arrange
        var dto = new BoardCreateDto { Name = "New Board" };
        var createdBoard = new Board { Id = 10, Name = dto.Name, UserId = TestUserId };

        _mockRepo.Setup(repo => repo.CreateBoardAsync(It.IsAny<Board>()))
            .ReturnsAsync(createdBoard);

        // Act
        var result = await _sut.CreateBoard(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(BoardsController.GetBoard));
        createdResult.RouteValues!["id"].Should().Be(10);
        createdResult.Value.Should().Be(createdBoard);
    }

    [Fact]
    public async Task CreateBoard_WhenUserIdInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var invalidUser = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "abc")]));

        _sut.ControllerContext.HttpContext.User = invalidUser;

        // Act
        var result = await _sut.CreateBoard(new BoardCreateDto());

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    // ------------------------------------------------------------
    // UpdateBoard
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateBoard_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var boardId = 1;
        var dto = new BoardUpdateDto { Name = "Updated Name" };
        var updatedBoard = new Board { Id = boardId, Name = "Updated Name" };

        _mockRepo.Setup(repo => repo.UpdateBoardAsync(boardId, dto.Name, TestUserId))
            .ReturnsAsync(updatedBoard);

        // Act
        var result = await _sut.UpdateBoard(boardId, dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(updatedBoard);
    }

    [Fact]
    public async Task UpdateBoard_WhenBoardNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var boardId = 1;
        var dto = new BoardUpdateDto { Name = "Updated Name" };

        _mockRepo.Setup(repo => repo.UpdateBoardAsync(boardId, dto.Name, TestUserId))
            .ReturnsAsync((Board?)null); // Repo returns null if not found

        // Act
        var result = await _sut.UpdateBoard(boardId, dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateBoard_WhenUserIdInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var invalidUser = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "abc")]));

        _sut.ControllerContext.HttpContext.User = invalidUser;

        // Act
        var result = await _sut.UpdateBoard(1, new BoardUpdateDto());

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    // ------------------------------------------------------------
    // DeleteBoard
    // ------------------------------------------------------------
    [Fact]
    public async Task DeleteBoard_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        var boardId = 1;
        _mockRepo.Setup(repo => repo.DeleteBoardAsync(boardId, TestUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteBoard(boardId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteBoard_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var boardId = 1;
        _mockRepo.Setup(repo => repo.DeleteBoardAsync(boardId, TestUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteBoard(boardId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteBoard_WhenUserIdInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var invalidUser = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "not-valid")]));

        _sut.ControllerContext.HttpContext.User = invalidUser;

        // Act
        var result = await _sut.DeleteBoard(1);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }
}