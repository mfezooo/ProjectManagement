using FluentAssertions;
using Moq;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Models;
using ProjectManagement.Application.Features.Auth.Commands.Register;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Tests.Common;

namespace ProjectManagement.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private static RegisterCommandHandler CreateHandler(
        Infrastructure.Persistence.AppDbContext db,
        Mock<IPasswordHasher>? hasher = null,
        Mock<IJwtService>? jwt = null)
    {
        hasher ??= new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");

        jwt ??= new Mock<IJwtService>();
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(("test-jwt", DateTime.UtcNow.AddHours(1)));

        return new RegisterCommandHandler(db, hasher.Object, jwt.Object);
    }

    [Fact]
    public async Task Register_WithNewEmail_PersistsUserAndReturnsToken()
    {
        await using var db = TestDbContextFactory.Create();
        var handler = CreateHandler(db);

        var command = new RegisterCommand
        {
            Email = "new@example.com",
            Password = "password123",
            FullName = "New User"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be("test-jwt");
        result.User.Email.Should().Be("new@example.com");

        db.Users.Should().ContainSingle(u => u.Email == "new@example.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ThrowsConflictException()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "dup@example.com",
            FullName = "Existing",
            PasswordHash = "x",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        var command = new RegisterCommand
        {
            Email = "dup@example.com",
            Password = "password123",
            FullName = "New User"
        };

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
