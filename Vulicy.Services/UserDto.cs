namespace Vulicy.Services;

public record UserDto(string Id, string Username, string Email, string? Name, string? AvatarUrl, bool IsAdmin);
