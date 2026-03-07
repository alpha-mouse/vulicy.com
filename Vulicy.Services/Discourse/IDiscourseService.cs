namespace Vulicy.Services;

public enum DiscourseWebhookResult
{
    Success = 200,
    UnexpectedBody = 400,
    InvalidSignature = 401,
    IgnoredEvent = 204,
}

public interface IDiscourseService
{
    Task<string?> CreateFeatureTopic(int featureId, int userId);
    Task<string?> CreateDossierRecordTopic(int dossierRecordId, int userId);
    Task<DiscourseWebhookResult> ProcessWebhook(string? signatureHeader, string? eventTypeHeader, ArraySegment<byte> body);
}