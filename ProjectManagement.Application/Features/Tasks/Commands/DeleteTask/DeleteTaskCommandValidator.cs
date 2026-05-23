using FluentValidation;

namespace ProjectManagement.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}
