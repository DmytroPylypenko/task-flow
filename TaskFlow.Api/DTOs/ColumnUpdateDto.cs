using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs;

public class ColumnUpdateDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}