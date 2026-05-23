using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Projects.DTOs;
using ProjectManagement.Application.Features.Projects.Mappings;

namespace ProjectManagement.Application.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetProjectByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException();

        var project = await _db.Projects
            .AsNoTracking()
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

        return project.ToDto();
    }
}
