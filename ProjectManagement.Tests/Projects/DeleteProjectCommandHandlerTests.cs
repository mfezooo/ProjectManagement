using FluentAssertions;
using Moq;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Projects.Commands.DeleteProject;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;
using ProjectManagement.Tests.Common;

namespace ProjectManagement.Tests.Projects;

public class DeleteProjectCommandHandlerTests
{
    [Fact]
    public async Task Owner_CanDeleteProject()
    {
        await using var db = TestDbContextFactory.Create();
        var owner = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), Name = "P", UserId = owner, CreatedAt = DateTime.UtcNow };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = owner, Role = UserRole.User };
        var cache = new Mock<ICacheService>();
        var handler = new DeleteProjectCommandHandler(db, currentUser, cache.Object);

        await handler.Handle(new DeleteProjectCommand { Id = project.Id }, CancellationToken.None);

        db.Projects.Should().BeEmpty();
    }

    [Fact]
    public async Task NonOwnerNonAdmin_GetsForbidden()
    {
        await using var db = TestDbContextFactory.Create();
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), Name = "P", UserId = owner, CreatedAt = DateTime.UtcNow };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = other, Role = UserRole.User };
        var cache = new Mock<ICacheService>();
        var handler = new DeleteProjectCommandHandler(db, currentUser, cache.Object);

        var act = async () => await handler.Handle(
            new DeleteProjectCommand { Id = project.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        db.Projects.Should().HaveCount(1);
    }
}
