namespace TaskFlow.Api.DTOs;

/// <summary>
/// Represents a single task's new position during a reorder operation.
/// </summary>
public class TaskReorderDto
{
    public int TaskId { get; set; }
    public int NewPosition { get; set; }
}