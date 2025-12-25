using FluentAssertions;
using TaskFlow.Api.Data;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository;
using TaskFlow.Api.Tests.Data;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Api.Tests.Repository;

public class UserRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        // Create a new, clean in-memory database for each test
        _context = DbContextFactory.Create();
        _sut = new UserRepository(_context);
    }

    [Fact]
    public async Task AddUserAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashed_password"
        };

        // Act
        await _sut.AddUserAsync(user);

        // Assert
        var insertedUser = await _context.Users.FindAsync(user.Id);

        insertedUser.Should().NotBeNull();
        insertedUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        await _context.Users.AddAsync(new User { Email = email });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.UserExistsAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var result = await _sut.UserExistsAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserExistsAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        await _context.Users.AddAsync(new User { Email = "User@Email.com" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.UserExistsAsync("user@email.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FindUserByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Email = "findme@example.com", Name = "Find Me" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var foundUser = await _sut.FindUserByEmailAsync(user.Email);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser.Email.Should().Be(user.Email);
        foundUser.Name.Should().Be(user.Name);
    }

    [Fact]
    public async Task FindUserByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.FindUserByEmailAsync("missing@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindUserByEmailAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var user = new User { Email = "Case@Test.COM" };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.FindUserByEmailAsync("case@test.com");

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
    }
}