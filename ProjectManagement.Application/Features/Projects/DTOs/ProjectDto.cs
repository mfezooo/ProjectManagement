namespace ProjectManagement.Application.Features.Projects.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public int TaskCount { get; set; }
}
