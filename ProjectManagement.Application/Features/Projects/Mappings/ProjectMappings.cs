using ProjectManagement.Application.Features.Projects.DTOs;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Features.Projects.Mappings;

public static class ProjectMappings
{
    public static ProjectDto ToDto(this Project project) =>
        new()
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UserId = project.UserId,
            TaskCount = project.Tasks?.Count ?? 0
        };

    public static List<ProjectDto> ToDtoList(this IEnumerable<Project> projects) =>
        projects.Select(p => p.ToDto()).ToList();
}
