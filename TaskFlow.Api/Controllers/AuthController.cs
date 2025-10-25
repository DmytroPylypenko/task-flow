using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;
using TaskFlow.Api.Utilities;

namespace TaskFlow.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
/// <summary>
/// Handles user registration and authentication via JWT.
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Initializes a new instance of the AuthController.
    /// </summary>
    /// <param name="context">The database context for user operations.</param>
    /// <param name="passwordHasher">The service used for hashing and verifying passwords.</param>
    public AuthController(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }
    
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration data (Name, Email, Password).</param>
    /// <returns>A 201 Created status on success, or 400 Bad Request on error.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
    {
        // 1. Check for email conflict
        if (await _userRepository.UserExistsAsync(request.Email))
        {
            return BadRequest(new { Message = "Email already exists." });
        }

        // 2. Hash the password
        string passwordHash = _passwordHasher.Hash(request.Password);

        // 3. Create the new User model
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        // 4. Save to the database
        await _userRepository.AddUserAsync(user);

        // Return a success response
        return StatusCode(201, new { Message = "User registered successfully." });
    }
}