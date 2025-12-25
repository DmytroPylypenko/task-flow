using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskFlow.Api.Controllers;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;
using TaskFlow.Api.Services.IServices;
using TaskFlow.Api.Utilities;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenService = new Mock<ITokenService>();

        _sut = new AuthController(
            _mockUserRepo.Object,
            _mockPasswordHasher.Object,
            _mockTokenService.Object);
    }

    // ------------------------------------------------------------
    // Register
    // ------------------------------------------------------------
    [Fact]
    public async Task Register_WhenEmailExists_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new UserRegisterDto { Name = "John", Email = "test@test.com", Password = "12345" };

        _mockUserRepo.Setup(r => r.UserExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Register(request);

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;

        var message = bad.Value!.GetType()
            .GetProperty("Message")!
            .GetValue(bad.Value);

        message.Should().Be("Email already exists.");

        _mockUserRepo.Verify(r => r.UserExistsAsync(request.Email), Times.Once);
        _mockUserRepo.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Register_WhenEmailDoesNotExist_ShouldCreateUserAndReturn201()
    {
        // Arrange
        var request = new UserRegisterDto
        {
            Name = "John",
            Email = "test@test.com",
            Password = "12345"
        };

        _mockUserRepo.Setup(r => r.UserExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockPasswordHasher.Setup(h => h.Hash(request.Password))
            .Returns("hashed_password");

        // Act
        var result = await _sut.Register(request);

        // Assert
        var created = result.Should().BeOfType<ObjectResult>().Subject;
        created.StatusCode.Should().Be(201);

        var message = created.Value!.GetType()
            .GetProperty("Message")!
            .GetValue(created.Value);

        message.Should().Be("User registered successfully.");

        _mockUserRepo.Verify(r => r.AddUserAsync(It.Is<User>(
            u => u.Email == request.Email && u.PasswordHash == "hashed_password")), Times.Once);
    }

    // ------------------------------------------------------------
    // Login
    // ------------------------------------------------------------
    [Fact]
    public async Task Login_WhenUserNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new UserLoginDto { Email = "missing@mail.com", Password = "123" };

        _mockUserRepo.Setup(r => r.FindUserByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Login(request);

        // Assert
        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;

        var message = unauthorized.Value!.GetType()
            .GetProperty("Message")!
            .GetValue(unauthorized.Value);

        message.Should().Be("Invalid credentials.");

        _mockUserRepo.Verify(r => r.FindUserByEmailAsync(request.Email), Times.Once);
    }

    [Fact]
    public async Task Login_WhenPasswordInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new UserLoginDto { Email = "john@mail.com", Password = "wrong" };

        var user = new User { Email = request.Email, PasswordHash = "hash" };

        _mockUserRepo.Setup(r => r.FindUserByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(h => h.Verify(user.PasswordHash, request.Password))
            .Returns(false);

        // Act
        var result = await _sut.Login(request);

        // Assert
        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;

        var message = unauthorized.Value!.GetType()
            .GetProperty("Message")!
            .GetValue(unauthorized.Value);

        message.Should().Be("Invalid credentials.");

        _mockPasswordHasher.Verify(h => h.Verify(user.PasswordHash, request.Password), Times.Once);
    }

    [Fact]
    public async Task Login_WhenCredentialsValid_ShouldReturnOkWithToken()
    {
        // Arrange
        var request = new UserLoginDto { Email = "john@mail.com", Password = "12345" };

        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash" };

        _mockUserRepo.Setup(r => r.FindUserByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(h => h.Verify(user.PasswordHash, request.Password))
            .Returns(true);

        _mockTokenService.Setup(t => t.CreateToken(user))
            .Returns("fake-jwt-token");

        // Act
        var result = await _sut.Login(request);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;

        var token = ok.Value!.GetType()
            .GetProperty("Token")!
            .GetValue(ok.Value) as string;

        token.Should().Be("fake-jwt-token");

        _mockTokenService.Verify(t => t.CreateToken(user), Times.Once);
    }
}