using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs;

/// <summary>
/// Data Transfer Object for moving a task to a new column.
/// </summary>
public class TaskMoveDto
{
    [Required]
    public int NewColumnId { get; set; }
}