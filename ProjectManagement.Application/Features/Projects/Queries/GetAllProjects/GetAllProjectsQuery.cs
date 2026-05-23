using MediatR;
using ProjectManagement.Application.Features.Projects.DTOs;

namespace ProjectManagement.Application.Features.Projects.Queries.GetAllProjects;

public class GetAllProjectsQuery : IRequest<List<ProjectDto>>
{
}
