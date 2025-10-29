using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs;

/// <summary>
/// Data Transfer Object for creating a new board.
/// </summary>
public class BoardCreateDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}