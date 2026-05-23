using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Projects.DTOs;
using ProjectManagement.Application.Features.Projects.Mappings;

namespace ProjectManagement.Application.Features.Projects.Queries.GetAllProjects;

public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, List<ProjectDto>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public GetAllProjectsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _db = db;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<List<ProjectDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        // Admin sees all (not cached per-admin to keep things simple and consistent).
        if (_currentUser.IsAdmin)
        {
            var allProjects = await _db.Projects
                .AsNoTracking()
                .Include(p => p.Tasks)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            return allProjects.ToDtoList();
        }

        var cacheKey = $"projects:user:{userId}";

        var cached = await _cache.GetAsync<List<ProjectDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var dto = projects.ToDtoList();

        await _cache.SetAsync(cacheKey, dto, CacheTtl, cancellationToken);

        return dto;
    }
}
