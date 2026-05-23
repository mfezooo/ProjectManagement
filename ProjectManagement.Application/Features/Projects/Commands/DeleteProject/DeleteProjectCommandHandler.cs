using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;

namespace ProjectManagement.Application.Features.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public DeleteProjectCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Project), request.Id);
        }

        if (!_currentUser.IsAdmin && project.UserId != userId)
        {
            throw new ForbiddenException();
        }

        var ownerId = project.UserId;

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"projects:user:{ownerId}", cancellationToken);
        await _cache.RemoveAsync($"tasks:project:{project.Id}", cancellationToken);

        return Unit.Value;
    }
}
