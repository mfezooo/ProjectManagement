using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Tasks.DTOs;
using ProjectManagement.Application.Features.Tasks.Mappings;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateTaskCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(nameof(Project), request.ProjectId);
        }

        if (!_currentUser.IsAdmin && project.UserId != userId)
        {
            throw new ForbiddenException();
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            DueDate = request.DueDate,
            Priority = request.Priority,
            Status = TaskItemStatus.ToDo,
            ProjectId = project.Id
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"tasks:project:{project.Id}", cancellationToken);
        await _cache.RemoveAsync($"projects:user:{project.UserId}", cancellationToken);

        return task.ToDto();
    }
}
