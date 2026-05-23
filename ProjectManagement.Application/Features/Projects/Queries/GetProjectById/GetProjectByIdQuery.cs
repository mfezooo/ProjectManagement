using MediatR;
using ProjectManagement.Application.Features.Projects.DTOs;

namespace ProjectManagement.Application.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQuery : IRequest<ProjectDto>
{
    public Guid Id { get; set; }
}
