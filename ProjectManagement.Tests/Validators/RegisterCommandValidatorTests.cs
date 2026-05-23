using FluentAssertions;
using ProjectManagement.Application.Features.Auth.Commands.Register;

namespace ProjectManagement.Tests.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_Passes()
    {
        var command = new RegisterCommand
        {
            Email = "ok@example.com",
            Password = "password123",
            FullName = "Jane Doe"
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "password123", "Name")]            // empty email
    [InlineData("notanemail", "password123", "Name")]   // invalid email
    [InlineData("ok@example.com", "short", "Name")]     // short password
    [InlineData("ok@example.com", "password123", "")]   // empty name
    public void Validate_WithInvalidInput_Fails(string email, string password, string name)
    {
        var result = _validator.Validate(new RegisterCommand
        {
            Email = email,
            Password = password,
            FullName = name
        });

        result.IsValid.Should().BeFalse();
    }
}
