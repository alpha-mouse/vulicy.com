using System.Text.RegularExpressions;
using Vulicy.Domain;

namespace Vulicy.Services;

public static partial class NameHelpers
{
    public static (FeatureType Type, string Name) ParseOsmCyrillicName(string fullName)
    {
        var match = GetOsmNameRegex().Match(fullName);
        if (!match.Success)
            return (FeatureType.Unknown, fullName);
        var typeGroup = match.Groups["type"];
        var type = ToFeatureType(typeGroup.Value);
        var name = match.Groups["name"].Value;
        return (type, name);
    }

    public static FeatureType ToFeatureType(string prefix)
    {
        return prefix switch
        {
            "вуліца" or "вул" or "вул." or "улица" or "ул" or "ул" => FeatureType.Street,
            "праспэкт" or "праспект" or "пр-т" or "пр" or "пр." or "проспект" => FeatureType.Avenue,
            "плошча" or "пл" or "пл." or "площадь" => FeatureType.Square,
            "бульвар" or "б-р" => FeatureType.Boulevard,
            "тракт" => FeatureType.HighRoad,
            "набярэжная" or "наб" or "наб." or "набережная" => FeatureType.Riverside,
            "шаша" or "шоссе" => FeatureType.Highway,
            "кальцо" or "кольцо" => FeatureType.Roundabout,
            "завулак" or "зав" or "зав." or "переулок" or "пер" or "пер." => FeatureType.Alley,
            "праезд" or "пр-д" or "проезд" => FeatureType.Driveway,
            "тупік" or "тупик" => FeatureType.DeadEnd,
            "спуск" => FeatureType.Descent,
            "заезд" => FeatureType.Entryway,
            "парк" => FeatureType.Park,
            "сквер" or "сквэр" => FeatureType.PublicGarden,
            _ => FeatureType.Unknown,
        };
    }

    public static string GetLabel(FeatureType type)
    {
        return type switch
        {
            FeatureType.Street => "вул.",
            FeatureType.Avenue => "пр-т",
            FeatureType.Square => "пл.",
            FeatureType.Boulevard => "бульв.",
            FeatureType.HighRoad => "тракт",
            FeatureType.Riverside => "наб.",
            FeatureType.Highway => "шаша",
            FeatureType.Roundabout => "кальцо",
            FeatureType.Alley => "зав.",
            FeatureType.Driveway => "пр-зд",
            FeatureType.DeadEnd => "тупік",
            FeatureType.Descent => "спуск",
            FeatureType.Entryway => "заезд",
            FeatureType.Park => "парк",
            FeatureType.PublicGarden => "сквэр",
            _ => ""
        };
    }

    public static bool IsSimilar(string s1, string s2, double threshold)
    {
        return threshold <= GetSimilarity(s1, s2);
    }

    public static int GetLevenshteinDistance(string s1, string s2)
    {
        return Fastenshtein.Levenshtein.Distance(s1, s2);
    }

    public static (int distance, int maxLength) GetLevenshteinDistanceWithMaxLength(string s1, string s2)
    {
        var distance = Fastenshtein.Levenshtein.Distance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        return (distance, maxLength);
    }

    public static double GetSimilarity(string s1, string s2)
    {
        var (distance, maxLength) = GetLevenshteinDistanceWithMaxLength(s1, s2);
        if (maxLength == 0) return 1.0;
        return 1.0 - (double)distance / maxLength;
    }

    public static (int distance, int maxLength) GetLevenshteinDistanceWithMaxLength(Fastenshtein.Levenshtein s1, string s2)
    {
        var distance = s1.DistanceFrom(s2);
        var maxLength = Math.Max(s1.StoredLength, s2.Length);
        return (distance, maxLength);
    }

    public static double GetSimilarity(Fastenshtein.Levenshtein s1, string s2)
    {
        var (distance, maxLength) = GetLevenshteinDistanceWithMaxLength(s1, s2);
        if (maxLength == 0) return 1.0;
        return 1.0 - (double)distance / maxLength;
    }

    private const string TypePattern
        = @"(?<type>"
          + @"вуліца|вул\.?|улица|ул\.?|"
          + @"праспэкт|праспект|пр-т|пр\.?|проспект|"
          + @"плошча|пл\.?|площадь|"
          + @"бульвар|б-р|"
          + @"тракт|"
          + @"набярэжная|наб\.|набережная|"
          + @"шаша|шоссе|"
          + @"кальцо|кольцо|"
          + @"завулак|зав\.?|переулок|пер\.|"
          + @"праезд|пр-д|проезд|"
          + @"тупік|тупик|"
          + @"спуск|"
          + @"заезд|"
          + @"парк|"
          + @"сквер|сквэр"
          + ")";
    [GeneratedRegex($@"^(?:{TypePattern} (?<name>.*)|(?<name>.*) {TypePattern})$")]
    private static partial Regex GetOsmNameRegex();
}