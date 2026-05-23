namespace ProjectManagement.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
