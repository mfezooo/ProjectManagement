using MediatR;
using ProjectManagement.Application.Features.Tasks.DTOs;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommand : IRequest<TaskDto>
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}
