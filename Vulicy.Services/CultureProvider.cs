using System.Globalization;

namespace Vulicy.Services;

internal static class CultureProvider
{
    public static readonly CultureInfo BeByCultureInfo = CultureInfo.GetCultureInfo("be-by");

    public static readonly StringComparer BeByStringComparer = BeByCultureInfo.CompareInfo.GetStringComparer(CompareOptions.IgnoreCase);
}