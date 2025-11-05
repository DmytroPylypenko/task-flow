using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing board.
/// </summary>
public class BoardUpdateDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}