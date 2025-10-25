using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs;

public class UserRegisterDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)] 
    public string Password { get; set; } = string.Empty;
}