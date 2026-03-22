using System.Globalization;

namespace Vulicy.Services;

public static class CultureProvider
{
    public static readonly CultureInfo BeByCultureInfo = CultureInfo.GetCultureInfo("be-by");

    public static readonly StringComparer BeByStringComparer = BeByCultureInfo.CompareInfo.GetStringComparer(CompareOptions.IgnoreCase);
}