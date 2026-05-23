using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = default!;
}
