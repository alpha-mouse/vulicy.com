namespace Vulicy.DB;

public static partial class DatabaseHelpers
{
    public static string CleanQuery(string? query)
    {
        return SearchQueryCleanupRegex()
            .Replace(query ?? "", m => m.Value == "*" ? "%" : "")
            .Trim();
    }

    public static IList<string> CleanQueryTerms(string? query)
    {
        return SearchQueryCleanupRegex()
            .Replace(query ?? "", m => m.Value == "*" ? "%" : "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    [System.Text.RegularExpressions.GeneratedRegex("[%_*]")]
    private static partial System.Text.RegularExpressions.Regex SearchQueryCleanupRegex();
}