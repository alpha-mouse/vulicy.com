namespace Vulicy.Services;

public interface IForumService
{
    Task<string?> CreateTopic(int featureId, int userId);
}
