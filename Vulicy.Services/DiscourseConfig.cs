namespace Vulicy.Services;

public class DiscourseConfig
{
    public string BaseUrl { get; set; }
    public string AuthSecret { get; set; }
    public string? ApiKey { get; set; }
    public int? ForumCategoryId { get; set; }
}