using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Projects.DTOs;
using ProjectManagement.Application.Features.Projects.Mappings;

namespace ProjectManagement.Application.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateProjectCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var project = await _db.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Project), request.Id);
        }

        if (!_currentUser.IsAdmin && project.UserId != userId)
        {
            throw new ForbiddenException();
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"projects:user:{project.UserId}", cancellationToken);

        return project.ToDto();
    }
}
