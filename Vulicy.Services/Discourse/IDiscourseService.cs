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
    Task<string?> CreateTopic(int featureId, int userId);
    Task<DiscourseWebhookResult> ProcessWebhook(string? signatureHeader, string? eventTypeHeader, ArraySegment<byte> body);
}