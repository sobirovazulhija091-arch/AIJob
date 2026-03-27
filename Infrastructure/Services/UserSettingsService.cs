using System.Net;
using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Identity;

public class UserSettingsService(UserManager<User> userManager) : IUserSettingsService
{
    private const string ThemeClaim = "settings.theme";
    private const string BrandClaim = "settings.brandColor";
    private const string EmailClaim = "settings.emailNotifications";
    private const string PushClaim = "settings.pushNotifications";
    private const string LanguageClaim = "settings.language";

    public async Task<Response<UserSettingsDto>> GetByUserIdAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new Response<UserSettingsDto>(HttpStatusCode.NotFound, "User not found");

        var claims = await userManager.GetClaimsAsync(user);

        var result = new UserSettingsDto
        {
            Theme = GetClaimValue(claims, ThemeClaim) ?? "light",
            BrandColor = GetClaimValue(claims, BrandClaim) ?? "blue",
            EmailNotifications = TryParseBool(GetClaimValue(claims, EmailClaim), true),
            PushNotifications = TryParseBool(GetClaimValue(claims, PushClaim), true),
            Language = GetClaimValue(claims, LanguageClaim) ?? "en"
        };

        return new Response<UserSettingsDto>(HttpStatusCode.OK, "ok", result);
    }

    public async Task<Response<string>> UpdateByUserIdAsync(int userId, UpdateUserSettingsDto dto)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new Response<string>(HttpStatusCode.NotFound, "User not found");

        var claims = await userManager.GetClaimsAsync(user);

        await UpsertClaimAsync(user, claims, ThemeClaim, dto.Theme);
        await UpsertClaimAsync(user, claims, BrandClaim, dto.BrandColor);
        await UpsertClaimAsync(user, claims, EmailClaim, dto.EmailNotifications.ToString().ToLowerInvariant());
        await UpsertClaimAsync(user, claims, PushClaim, dto.PushNotifications.ToString().ToLowerInvariant());
        await UpsertClaimAsync(user, claims, LanguageClaim, dto.Language);

        return new Response<string>(HttpStatusCode.OK, "Settings updated");
    }

    private async Task UpsertClaimAsync(User user, IList<System.Security.Claims.Claim> existingClaims, string type, string value)
    {
        var existing = existingClaims.FirstOrDefault(c => c.Type == type);
        var newClaim = new System.Security.Claims.Claim(type, value);
        if (existing == null)
            await userManager.AddClaimAsync(user, newClaim);
        else
            await userManager.ReplaceClaimAsync(user, existing, newClaim);
    }

    private static string? GetClaimValue(IList<System.Security.Claims.Claim> claims, string claimType)
        => claims.FirstOrDefault(c => c.Type == claimType)?.Value;

    private static bool TryParseBool(string? raw, bool fallback)
        => bool.TryParse(raw, out var v) ? v : fallback;
}
