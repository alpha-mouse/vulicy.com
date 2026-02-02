using System.Text.RegularExpressions;
using Vulicy.Domain;

namespace Vulicy.Services;

public interface ILinksService
{
    string CreateFeatureLink(int featureId, ForumTopicData data);
    bool TryFindLinkAndParseFeatureId(string? text, out int featureId);
}

public class LinksService(AppConfig appConfig) : ILinksService
{
    private readonly Regex _featureIdRegex = new($@"{appConfig.BaseUrl}/(?:\?featureId=(?<featureId>\d+)|\?\S+&featureId=(?<featureId>\d+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string CreateFeatureLink(int featureId, ForumTopicData data)
    {
        return $"{appConfig.BaseUrl}/?featureId={featureId}&lat={data.Lat:F4}&lng={data.Lng:F4}&z={appConfig.DefaultZoom}";
    }

    public bool TryFindLinkAndParseFeatureId(string? text, out int featureId)
    {
        if (text == null)
        {
            featureId = default;
            return false;
        }

        var match = _featureIdRegex.Match(text);
        if (match.Success)
        {
            return Int32.TryParse(match.Groups["featureId"].Value, out featureId);
        }

        featureId = default;
        return false;
    }
}