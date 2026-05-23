using MediatR;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Projects.DTOs;
using ProjectManagement.Application.Features.Projects.Mappings;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateProjectCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Tasks = new List<TaskItem>()
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"projects:user:{userId}", cancellationToken);

        return project.ToDto();
    }
}
