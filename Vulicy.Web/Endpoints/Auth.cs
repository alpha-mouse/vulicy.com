using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Vulicy.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Vulicy.Web.Endpoints;

public static class Auth
{
    public const string AdminRole = "Admin";

    public static void MapAuth(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/auth");
        group.MapGet("/login", Login);
        group.MapGet("/callback", Callback);
        group.MapGet("/logout", Logout);
        group.MapGet("/me", Me);
    }

    private static IResult Login(string? returnUrl, IDiscourseConnectService discourseConnectService, HttpContext context)
    {
        var nonce = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
        context.Response.Cookies.Append("sso_nonce", nonce, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10)
        });

        var request = context.Request;
        var callbackUrl = $"{request.Scheme}://{request.Host}/api/auth/callback";
        if (!string.IsNullOrEmpty(returnUrl))
        {
            callbackUrl += $"?finalRedirect={WebUtility.UrlEncode(returnUrl)}";
        }

        var loginUrl = discourseConnectService.CreateLoginUrl(callbackUrl, nonce);
        return Results.Redirect(loginUrl);
    }

    private static async Task<IResult> Callback(
        [FromQuery] string sso,
        [FromQuery] string sig,
        [FromQuery] string? finalRedirect,
        IDiscourseConnectService discourseConnectService,
        HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue("sso_nonce", out var nonce) || string.IsNullOrEmpty(nonce))
        {
            return Results.BadRequest("Missing or expired nonce.");
        }

        var user = await discourseConnectService.VerifyAndGetOrCreateUser(sso, sig, nonce);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, AdminRole));
        }

        if (!string.IsNullOrEmpty(user.Name)) claims.Add(new Claim("name", user.Name));
        if (!string.IsNullOrEmpty(user.AvatarUrl)) claims.Add(new Claim("avatar_url", user.AvatarUrl));

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true
        };

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        context.Response.Cookies.Delete("sso_nonce");

        return Results.Redirect(GetLocalRedirect(finalRedirect));
    }

    // Only allow site-relative redirects so a crafted returnUrl cannot turn login into an open redirect.
    private static string GetLocalRedirect(string? url)
    {
        if (string.IsNullOrEmpty(url) || url[0] != '/')
            return "/";
        // Reject protocol-relative ("//host") and backslash-tricks ("/\host") that browsers treat as absolute.
        if (url.Length > 1 && (url[1] == '/' || url[1] == '\\'))
            return "/";
        return url;
    }

    private static IResult Logout()
    {
        return Results.SignOut(new AuthenticationProperties { RedirectUri = "/" }, [CookieAuthenticationDefaults.AuthenticationScheme]);
    }

    private static IResult Me(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new UserDto(
            context.User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            context.User.Identity.Name!,
            context.User.FindFirstValue(ClaimTypes.Email)!,
            context.User.FindFirstValue("name"),
            context.User.FindFirstValue("avatar_url"),
            context.User.IsInRole(AdminRole)
        ));
    }
}
