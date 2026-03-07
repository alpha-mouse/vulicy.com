using System.Text.RegularExpressions;
using Vulicy.Domain;

namespace Vulicy.Services;

public interface ILinksService
{
    string CreateFeatureLink(int featureId, FeatureForumTopicData data);
    string CreateDossierRecordLink(int dossierRecordId);
    bool TryFindFeatureLinkAndParseId(string? text, out int featureId);
    bool TryFindDossierRecordLinkAndParseId(string? text, out int dossierRecordId);
}

public class LinksService(AppConfig appConfig) : ILinksService
{
    private readonly Regex _featureIdRegex = new($@"{appConfig.BaseUrl}/(?:\?featureId=(?<featureId>\d+)|\?\S+&featureId=(?<featureId>\d+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly Regex _dossierRecordIdRegex = new($@"{appConfig.BaseUrl}/(?:\?dossierRecordId=(?<dossierRecordId>\d+)|\?\S+&dossierRecordId=(?<dossierRecordId>\d+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string CreateFeatureLink(int featureId, FeatureForumTopicData data)
    {
        return $"{appConfig.BaseUrl}/?featureId={featureId}&lat={data.Lat:F4}&lng={data.Lng:F4}&z={appConfig.DefaultZoom}";
    }

    public string CreateDossierRecordLink(int dossierRecordId)
    {
        return $"{appConfig.BaseUrl}/?dossierRecordId={dossierRecordId}";
    }

    public bool TryFindFeatureLinkAndParseId(string? text, out int featureId)
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

    public bool TryFindDossierRecordLinkAndParseId(string? text, out int dossierRecordId)
    {
        if (text == null)
        {
            dossierRecordId = default;
            return false;
        }

        var match = _dossierRecordIdRegex.Match(text);
        if (match.Success)
        {
            return Int32.TryParse(match.Groups["dossierRecordId"].Value, out dossierRecordId);
        }

        dossierRecordId = default;
        return false;
    }
}