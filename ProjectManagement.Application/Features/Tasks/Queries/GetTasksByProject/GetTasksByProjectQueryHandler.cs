using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Tasks.DTOs;
using ProjectManagement.Application.Features.Tasks.Mappings;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Features.Tasks.Queries.GetTasksByProject;

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, List<TaskDto>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public GetTasksByProjectQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<List<TaskDto>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException(nameof(Project), request.ProjectId);
        }

        if (!_currentUser.IsAdmin && project.UserId != userId)
        {
            throw new ForbiddenException();
        }

        var cacheKey = $"tasks:project:{request.ProjectId}";

        var cached = await _cache.GetAsync<List<TaskDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var tasks = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == request.ProjectId)
            .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
            .ToListAsync(cancellationToken);

        var dto = tasks.ToDtoList();

        await _cache.SetAsync(cacheKey, dto, CacheTtl, cancellationToken);

        return dto;
    }
}
