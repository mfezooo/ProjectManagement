using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Features.Tasks.DTOs;

public class CreateTaskRequest
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}
