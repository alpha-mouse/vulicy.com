namespace Vulicy.DB;

public static partial class DatabaseHelpers
{
    public static string CleanQuery(string? query)
    {
        return SearchQueryCleanupRegex()
            .Replace(query ?? "", m => m.Value == "*" ? "%" : "")
            .Trim();
    }

    [System.Text.RegularExpressions.GeneratedRegex("[%_*]")]
    private static partial System.Text.RegularExpressions.Regex SearchQueryCleanupRegex();
}