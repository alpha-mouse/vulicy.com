namespace Vulicy.DB;

public static partial class DatabaseHelpers
{
    public static string CleanQuery(string? query)
    {
        return SearchQueryCleanupRegex()
            .Replace(query ?? "", "")
            .Trim();
    }

    public static IList<string> CleanQueryTerms(string? query)
    {
        return SearchQueryCleanupRegex()
            .Replace(query ?? "", "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    [System.Text.RegularExpressions.GeneratedRegex("[%_*]")]
    private static partial System.Text.RegularExpressions.Regex SearchQueryCleanupRegex();
}