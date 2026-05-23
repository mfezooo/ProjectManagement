namespace ProjectManagement.Application.Features.Projects.DTOs;

public class UpdateProjectRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
