using FluentAssertions;
using Moq;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Tasks.Commands.UpdateTaskStatus;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;
using ProjectManagement.Tests.Common;

namespace ProjectManagement.Tests.Tasks;

public class UpdateTaskStatusCommandHandlerTests
{
    [Fact]
    public async Task Owner_CanUpdateStatus()
    {
        await using var db = TestDbContextFactory.Create();
        var owner = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), Name = "P", UserId = owner, CreatedAt = DateTime.UtcNow };
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "T",
            Status = TaskItemStatus.ToDo,
            Priority = TaskPriority.Low,
            ProjectId = project.Id,
            Project = project
        };
        db.Projects.Add(project);
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = owner };
        var cache = new Mock<ICacheService>();
        var handler = new UpdateTaskStatusCommandHandler(db, currentUser, cache.Object);

        var result = await handler.Handle(new UpdateTaskStatusCommand
        {
            TaskId = task.Id,
            Status = TaskItemStatus.Done
        }, CancellationToken.None);

        result.Status.Should().Be(TaskItemStatus.Done);
        cache.Verify(c => c.RemoveAsync($"tasks:project:{project.Id}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
