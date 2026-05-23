using MediatR;
using ProjectManagement.Application.Features.Tasks.DTOs;

namespace ProjectManagement.Application.Features.Tasks.Queries.GetTasksByProject;

public class GetTasksByProjectQuery : IRequest<List<TaskDto>>
{
    public Guid ProjectId { get; set; }
}
