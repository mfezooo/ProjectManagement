using MediatR;
using ProjectManagement.Application.Features.Projects.DTOs;

namespace ProjectManagement.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommand : IRequest<ProjectDto>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
