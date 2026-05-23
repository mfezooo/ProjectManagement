using MediatR;
using ProjectManagement.Application.Common.Models;

namespace ProjectManagement.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
