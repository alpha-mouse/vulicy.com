namespace Vulicy.Domain;

public class UserEntity : Entity<int>
{
    public int ExternalId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsAdmin { get; set; }
}
