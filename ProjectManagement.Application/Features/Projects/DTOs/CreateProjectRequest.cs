namespace ProjectManagement.Application.Features.Projects.DTOs;

public class CreateProjectRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
