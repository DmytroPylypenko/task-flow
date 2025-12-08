using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs;

/// <summary>
/// Data Transfer Object used for user login (sign in).
/// </summary>
public class UserLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}