using System.Security.Cryptography;
using System.Text;
using Vulicy.Domain;
using System.Net;

namespace Vulicy.Services;

public class DiscourseConnectService(AuthConfig config, IUserRepository userRepository) : IDiscourseConnectService
{
    public string CreateLoginUrl(string returnUrl, string nonce)
    {
        var payload = $"nonce={nonce}&return_sso_url={WebUtility.UrlEncode(returnUrl)}";
        var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        var signature = CalculateSignature(base64Payload);

        return $"{config.DiscourseBaseUrl}/session/sso_provider?sso={WebUtility.UrlEncode(base64Payload)}&sig={signature}";
    }

    public async Task<UserEntity?> VerifyAndGetOrCreateUser(string payload, string signature, string nonce)
    {
        if (CalculateSignature(payload) != signature)
        {
            return null;
        }

        var decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        var parameters = ParseQueryString(decodedPayload);

        if (parameters.GetValueOrDefault("nonce") != nonce)
        {
            return null;
        }

        var externalIdStr = parameters.GetValueOrDefault("external_id");
        if (string.IsNullOrEmpty(externalIdStr) || !int.TryParse(externalIdStr, out var externalId))
        {
            return null;
        }

        var email = parameters.GetValueOrDefault("email") ?? string.Empty;
        var username = parameters.GetValueOrDefault("username") ?? string.Empty;
        var name = parameters.GetValueOrDefault("name");
        var avatarUrl = parameters.GetValueOrDefault("avatarUrl"); // Discourse sometimes uses avatar_url or avatarUrl depending on version/config, but usually avatar_url
        if (string.IsNullOrEmpty(avatarUrl)) avatarUrl = parameters.GetValueOrDefault("avatar_url");

        var isAdmin = parameters.GetValueOrDefault("admin") == "true";

        var user = await userRepository.GetByExternalIdTracking(externalId);
        if (user == null)
        {
            user = new UserEntity
            {
                ExternalId = externalId,
                Username = username,
                Email = email,
                Name = name,
                AvatarUrl = avatarUrl,
                IsAdmin = isAdmin
            };
            userRepository.Add(user);
        }
        else
        {
            user.Username = username;
            user.Email = email;
            user.Name = name;
            user.AvatarUrl = avatarUrl;
            user.IsAdmin = isAdmin;
        }

        await userRepository.SaveChanges();
        return user;
    }

    private string CalculateSignature(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.DiscourseSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLower();
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        return query.Split('&')
            .Select(p => p.Split('='))
            .ToDictionary(
                split => WebUtility.UrlDecode(split[0]),
                split => split.Length > 1 ? WebUtility.UrlDecode(split[1]) : string.Empty
            );
    }
}
