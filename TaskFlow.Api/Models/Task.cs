using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskFlow.Api.Models;

public class Task
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Position { get; set; }
    
    public int ColumnId { get; set; }
    
    [JsonIgnore]
    public Column? Column { get; set; }
}