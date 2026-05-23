namespace ProjectManagement.Application.Common.Models;

public class AuthResponse
{
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public AuthUserDto User { get; set; } = default!;
}

public class AuthUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Role { get; set; } = default!;
}
