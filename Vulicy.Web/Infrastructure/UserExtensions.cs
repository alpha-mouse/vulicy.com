using System.Security.Claims;

namespace Vulicy.Web.Infrastructure;

public static class UserExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public int GetUserId()
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out var userId) ? userId : throw new InvalidOperationException("User Id not found");
        }
    }
}