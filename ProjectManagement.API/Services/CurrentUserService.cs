using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Domain.Enums;

namespace ProjectManagement.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var principal = Principal;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => Principal?.FindFirst(ClaimTypes.Email)?.Value
                            ?? Principal?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    public UserRole? Role
    {
        get
        {
            var raw = Principal?.FindFirst(ClaimTypes.Role)?.Value
                      ?? Principal?.FindFirst("role")?.Value;
            return Enum.TryParse<UserRole>(raw, ignoreCase: true, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public bool IsAdmin => Role == UserRole.Admin;
}
