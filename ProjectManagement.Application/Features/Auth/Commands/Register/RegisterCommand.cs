using MediatR;
using ProjectManagement.Application.Common.Models;

namespace ProjectManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FullName { get; set; } = default!;
}
