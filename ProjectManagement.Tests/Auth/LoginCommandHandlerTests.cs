using FluentAssertions;
using Moq;
using ProjectManagement.Application.Common.Exceptions;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Features.Auth.Commands.Login;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Tests.Common;
using Xunit;

namespace ProjectManagement.Tests.Auth;

public class LoginCommandHandlerTests
{
    private static (LoginCommandHandler Handler, Mock<IPasswordHasher> Hasher, Mock<IJwtService> Jwt)
        Build(Infrastructure.Persistence.AppDbContext db)
    {
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtService>();
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

        return (new LoginCommandHandler(db, hasher.Object, jwt.Object), hasher, jwt);
    }

    private static User Seed(Infrastructure.Persistence.AppDbContext db, string email = "user@example.com")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = "Joe",
            PasswordHash = "stored-hash",
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        await using var db = TestDbContextFactory.Create();
        Seed(db);
        var (handler, hasher, _) = Build(db);

        hasher.Setup(h => h.Verify("good-pass", "stored-hash")).Returns(true);

        var result = await handler.Handle(
            new LoginCommand { Email = "user@example.com", Password = "good-pass" },
            CancellationToken.None);

        result.Token.Should().Be("jwt-token");
        result.User.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        await using var db = TestDbContextFactory.Create();
        Seed(db);
        var (handler, hasher, _) = Build(db);

        hasher.Setup(h => h.Verify("bad", "stored-hash")).Returns(false);

        var act = async () => await handler.Handle(
            new LoginCommand { Email = "user@example.com", Password = "bad" },
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_WithUnknownUser_ThrowsUnauthorized()
    {
        await using var db = TestDbContextFactory.Create();
        var (handler, _, _) = Build(db);

        var act = async () => await handler.Handle(
            new LoginCommand { Email = "ghost@example.com", Password = "x" },
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
