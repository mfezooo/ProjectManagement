using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Features.Tasks.DTOs;

public class UpdateTaskStatusRequest
{
    public TaskItemStatus Status { get; set; }
}
