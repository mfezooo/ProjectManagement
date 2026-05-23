using FluentAssertions;
using Moq;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Tasks.Commands.CreateTask;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;
using ProjectManagement.Tests.Common;

namespace ProjectManagement.Tests.Tasks;

public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Owner_CreatesTaskSuccessfully()
    {
        await using var db = TestDbContextFactory.Create();
        var owner = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), Name = "P", UserId = owner, CreatedAt = DateTime.UtcNow };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = owner };
        var cache = new Mock<ICacheService>();
        var handler = new CreateTaskCommandHandler(db, currentUser, cache.Object);

        var result = await handler.Handle(new CreateTaskCommand
        {
            ProjectId = project.Id,
            Title = "Task 1",
            Priority = TaskPriority.High
        }, CancellationToken.None);

        result.Title.Should().Be("Task 1");
        result.Priority.Should().Be(TaskPriority.High);
        result.Status.Should().Be(TaskItemStatus.ToDo);
        db.Tasks.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProjectNotFound_Throws()
    {
        await using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService { UserId = Guid.NewGuid() };
        var cache = new Mock<ICacheService>();
        var handler = new CreateTaskCommandHandler(db, currentUser, cache.Object);

        var act = async () => await handler.Handle(new CreateTaskCommand
        {
            ProjectId = Guid.NewGuid(),
            Title = "Task"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task NotOwner_GetsForbidden()
    {
        await using var db = TestDbContextFactory.Create();
        var owner = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), Name = "P", UserId = owner, CreatedAt = DateTime.UtcNow };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = Guid.NewGuid(), Role = UserRole.User };
        var cache = new Mock<ICacheService>();
        var handler = new CreateTaskCommandHandler(db, currentUser, cache.Object);

        var act = async () => await handler.Handle(new CreateTaskCommand
        {
            ProjectId = project.Id,
            Title = "X"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
