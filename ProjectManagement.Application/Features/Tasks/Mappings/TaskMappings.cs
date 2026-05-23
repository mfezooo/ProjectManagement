using ProjectManagement.Application.Features.Tasks.DTOs;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Features.Tasks.Mappings;

public static class TaskMappings
{
    public static TaskDto ToDto(this TaskItem task) =>
        new()
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDate = task.DueDate,
            Priority = task.Priority,
            ProjectId = task.ProjectId
        };

    public static List<TaskDto> ToDtoList(this IEnumerable<TaskItem> tasks) =>
        tasks.Select(t => t.ToDto()).ToList();
}
