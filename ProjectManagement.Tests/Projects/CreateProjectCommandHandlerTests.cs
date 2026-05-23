using FluentAssertions;
using Moq;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Projects.Commands.CreateProject;
using ProjectManagement.Tests.Common;

namespace ProjectManagement.Tests.Projects;

public class CreateProjectCommandHandlerTests
{
    [Fact]
    public async Task Create_WithAuthenticatedUser_PersistsProject()
    {
        await using var db = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var currentUser = new FakeCurrentUserService { UserId = userId };
        var cache = new Mock<ICacheService>();

        var handler = new CreateProjectCommandHandler(db, currentUser, cache.Object);

        var command = new CreateProjectCommand
        {
            Name = "My Project",
            Description = "Description"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("My Project");
        result.UserId.Should().Be(userId);

        db.Projects.Should().HaveCount(1);
        cache.Verify(c => c.RemoveAsync($"projects:user:{userId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithoutAuthenticatedUser_ThrowsUnauthorized()
    {
        await using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService { UserId = null };
        var cache = new Mock<ICacheService>();

        var handler = new CreateProjectCommandHandler(db, currentUser, cache.Object);

        var act = async () => await handler.Handle(
            new CreateProjectCommand { Name = "x" }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
