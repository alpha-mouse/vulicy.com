using System.Security.Claims;

namespace Vulicy.Web.Infrastructure;

public static class UserExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public int? UserId()
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}