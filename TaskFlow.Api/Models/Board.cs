using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Models;

public class Board
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Column> Columns { get; set; } = new List<Column>();
}