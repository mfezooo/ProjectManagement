using ProjectManagement.Domain.Enums;

namespace ProjectManagement.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
