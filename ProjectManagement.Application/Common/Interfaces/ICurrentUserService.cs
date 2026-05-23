using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
