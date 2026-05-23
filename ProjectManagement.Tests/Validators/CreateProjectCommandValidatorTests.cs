using FluentAssertions;
using ProjectManagement.Application.Features.Projects.Commands.CreateProject;

namespace ProjectManagement.Tests.Validators;

public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_Passes()
    {
        var result = _validator.Validate(new CreateProjectCommand
        {
            Name = "My project",
            Description = "Some description"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_Fails()
    {
        var result = _validator.Validate(new CreateProjectCommand
        {
            Name = string.Empty
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProjectCommand.Name));
    }

    [Fact]
    public void Validate_WithTooLongName_Fails()
    {
        var result = _validator.Validate(new CreateProjectCommand
        {
            Name = new string('x', 201)
        });

        result.IsValid.Should().BeFalse();
    }
}
