using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Tests.Common;

public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; } = UserRole.User;
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsAdmin => Role == UserRole.Admin;
}
