using FluentAssertions;
using Moq;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Projects.DTOs;
using ProjectManagement.Application.Features.Projects.Queries.GetAllProjects;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;
using ProjectManagement.Tests.Common;

namespace ProjectManagement.Tests.Projects;

public class GetAllProjectsQueryHandlerTests
{
    private static (Guid OwnerA, Guid OwnerB) Seed(Infrastructure.Persistence.AppDbContext db)
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        db.Users.AddRange(
            new User { Id = a, Email = "a@x.com", FullName = "A", PasswordHash = "h", CreatedAt = DateTime.UtcNow },
            new User { Id = b, Email = "b@x.com", FullName = "B", PasswordHash = "h", CreatedAt = DateTime.UtcNow });

        db.Projects.AddRange(
            new Project { Id = Guid.NewGuid(), Name = "A-1", UserId = a, CreatedAt = DateTime.UtcNow },
            new Project { Id = Guid.NewGuid(), Name = "A-2", UserId = a, CreatedAt = DateTime.UtcNow },
            new Project { Id = Guid.NewGuid(), Name = "B-1", UserId = b, CreatedAt = DateTime.UtcNow });

        db.SaveChanges();
        return (a, b);
    }

    [Fact]
    public async Task User_SeesOnlyOwnProjects()
    {
        await using var db = TestDbContextFactory.Create();
        var (a, _) = Seed(db);

        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetAsync<List<ProjectDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ProjectDto>?)null);

        var currentUser = new FakeCurrentUserService { UserId = a, Role = UserRole.User };
        var handler = new GetAllProjectsQueryHandler(db, currentUser, cache.Object);

        var result = await handler.Handle(new GetAllProjectsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.UserId == a);
    }

    [Fact]
    public async Task Admin_SeesAllProjects()
    {
        await using var db = TestDbContextFactory.Create();
        Seed(db);

        var cache = new Mock<ICacheService>();
        var currentUser = new FakeCurrentUserService { UserId = Guid.NewGuid(), Role = UserRole.Admin };
        var handler = new GetAllProjectsQueryHandler(db, currentUser, cache.Object);

        var result = await handler.Handle(new GetAllProjectsQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
    }
}
