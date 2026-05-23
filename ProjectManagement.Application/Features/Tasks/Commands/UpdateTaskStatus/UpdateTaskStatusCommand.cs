using MediatR;
using ProjectManagement.Application.Features.Tasks.DTOs;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Features.Tasks.Commands.UpdateTaskStatus;

public class UpdateTaskStatusCommand : IRequest<TaskDto>
{
    public Guid TaskId { get; set; }
    public TaskItemStatus Status { get; set; }
}
