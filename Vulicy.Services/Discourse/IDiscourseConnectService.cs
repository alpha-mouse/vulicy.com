namespace Vulicy.Services;

public interface IDiscourseConnectService
{
    string CreateLoginUrl(string returnUrl, string nonce);
    Task<Vulicy.Domain.UserEntity?> VerifyAndGetOrCreateUser(string payload, string signature, string nonce);
}
