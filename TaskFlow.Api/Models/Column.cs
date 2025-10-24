using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Models;

public class Column
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public int BoardId { get; set; }
    public Board? Board { get; set; }

    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}