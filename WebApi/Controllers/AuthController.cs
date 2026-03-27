using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Incorrect password" });
        }
    }

    [AllowAnonymous]
    [HttpGet("external/{provider}")]
    public IActionResult ExternalLogin([FromRoute] string provider, [FromQuery] string? returnUrl = null)
    {
        var scheme = provider.Equals("google", StringComparison.OrdinalIgnoreCase) ? "Google" :
                     provider.Equals("github", StringComparison.OrdinalIgnoreCase) ? "GitHub" : "";

        if (string.IsNullOrWhiteSpace(scheme))
            return BadRequest(new { message = "Unsupported provider. Use google or github." });

        var configured = !string.IsNullOrWhiteSpace(_configuration[$"Authentication:{scheme}:ClientId"]) &&
                         !string.IsNullOrWhiteSpace(_configuration[$"Authentication:{scheme}:ClientSecret"]);
        if (!configured)
            return BadRequest(new { message = $"{scheme} auth is not configured on server." });

        var callback = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl })!;
        var props = new AuthenticationProperties { RedirectUri = callback };
        props.Items["scheme"] = scheme;
        return Challenge(props, scheme);
    }

    [AllowAnonymous]
    [HttpGet("external-callback")]
    public async Task<IActionResult> ExternalLoginCallback([FromQuery] string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded || result.Principal is null)
            return BadRequest(new { message = "External authentication failed." });

        var provider = result.Properties?.Items.TryGetValue("scheme", out var s) == true ? s : null;
        if (string.IsNullOrWhiteSpace(provider))
            return BadRequest(new { message = "Provider information not found." });

        var providerKey = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? result.Principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(providerKey))
            return BadRequest(new { message = "External user id was not provided by provider." });

        var email = result.Principal.FindFirstValue(ClaimTypes.Email) ?? result.Principal.FindFirstValue("email");
        var fullName = result.Principal.FindFirstValue(ClaimTypes.Name) ?? result.Principal.FindFirstValue("name");

        var tokens = await _authService.ExternalLoginAsync(provider, providerKey, email, fullName);
        await HttpContext.SignOutAsync("External");

        var clientReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "http://localhost:5206/auth/callback" : returnUrl;
        var separator = clientReturnUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        var redirectUrl = $"{clientReturnUrl}{separator}token={Uri.EscapeDataString(tokens.Token)}&refreshToken={Uri.EscapeDataString(tokens.RefreshToken)}";
        return Redirect(redirectUrl);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        await _authService.LogoutAsync(dto.RefreshToken);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            await _authService.ForgotPasswordAsync(dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            await _authService.ResetPasswordAsync(dto);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

