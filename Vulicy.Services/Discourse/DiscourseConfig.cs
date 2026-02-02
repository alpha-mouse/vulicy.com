namespace Vulicy.Services;

public class DiscourseConfig
{
    public string BaseUrl { get; set; } = null!;
    public string AuthSecret { get; set; } = null!;
    public string? ApiKey { get; set; }
    public int StreetCategoryId { get; set; }
}