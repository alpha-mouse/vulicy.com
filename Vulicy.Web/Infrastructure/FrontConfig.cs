namespace Vulicy.Web.Infrastructure;

public class FrontConfig
{
    public string DiscourseBaseUrl { get; set; } = null!;
    public string MapKey { get; set; } = null!;
    public string SentryFeDsn { get; set; } = null!;
    public string Environment { get; set; } = null!;
}