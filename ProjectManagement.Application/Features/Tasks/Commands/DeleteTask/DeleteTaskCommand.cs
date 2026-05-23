using MediatR;

namespace ProjectManagement.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommand : IRequest<Unit>
{
    public Guid TaskId { get; set; }
}
