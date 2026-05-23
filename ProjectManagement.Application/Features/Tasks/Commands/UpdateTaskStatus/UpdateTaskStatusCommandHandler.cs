using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Tasks.DTOs;
using ProjectManagement.Application.Features.Tasks.Mappings;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Features.Tasks.Commands.UpdateTaskStatus;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, TaskDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateTaskStatusCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<TaskDto> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
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

        task.Status = request.Status;
        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"tasks:project:{task.ProjectId}", cancellationToken);

        return task.ToDto();
    }
}
