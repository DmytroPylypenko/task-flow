using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Models;
using TaskFlow.Api.Repository.IRepository;
using TaskFlow.Api.Services.IServices;
using TaskFlow.Api.Utilities;

namespace TaskFlow.Api.Controllers;

/// <summary>
/// Handles user registration and authentication via JWT.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
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

    /// <summary>
    /// Authenticates a user and returns a JWT.
    /// </summary>
    /// <param name="request">The login data (Email, Password).</param>
    /// <returns>A JWT on success, or 401 Unauthorized on failure.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {
        // 1. Find the user by email
        var user = await _userRepository.FindUserByEmailAsync(request.Email);
        if (user is null)
        {
            // Return Unauthorized to prevent leaking info about which emails exist
            return Unauthorized(new { Message = "Invalid credentials." });
        }

        // 2. Verify the password
        bool isPasswordValid = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!isPasswordValid)
        {
            return Unauthorized(new { Message = "Invalid credentials." });
        }

        // 3. Create the JWT
        string token = _tokenService.CreateToken(user);

        // 4. Return the token
        return Ok(new { Token = token });
    }
}