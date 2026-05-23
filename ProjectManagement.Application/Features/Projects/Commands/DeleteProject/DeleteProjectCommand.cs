using MediatR;

namespace ProjectManagement.Application.Features.Projects.Commands.DeleteProject;

public class DeleteProjectCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
