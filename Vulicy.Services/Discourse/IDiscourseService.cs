namespace Vulicy.Services;

public interface IDiscourseService
{
    Task<string?> CreateTopic(int featureId, int userId);
}
