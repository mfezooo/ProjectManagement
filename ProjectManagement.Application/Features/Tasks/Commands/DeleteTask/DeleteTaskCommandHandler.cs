using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public DeleteTaskCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var task = await _db.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException(nameof(TaskItem), request.TaskId);
        }

        if (!_currentUser.IsAdmin && task.Project.UserId != userId)
        {
            throw new ForbiddenException();
        }

        var projectId = task.ProjectId;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"tasks:project:{projectId}", cancellationToken);

        return Unit.Value;
    }
}
