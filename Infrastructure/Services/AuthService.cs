using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IEmailService emailService,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already in use");

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            PhoneNumber = registerDto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, registerDto.Role.ToString());

        var token = GenerateJwtToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user is null)
            throw new InvalidOperationException("Email does not exist");

        var check = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
        if (!check.Succeeded)
            throw new UnauthorizedAccessException("Incorrect password");

        var token = GenerateJwtToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> ExternalLoginAsync(string provider, string providerKey, string? email, string? fullName)
    {
        var user = await _userManager.FindByLoginAsync(provider, providerKey);

        if (user is null && !string.IsNullOrWhiteSpace(email))
        {
            user = await _userManager.FindByEmailAsync(email);
        }

        if (user is null)
        {
            var normalizedProvider = provider.ToLowerInvariant();
            var fallbackEmail = !string.IsNullOrWhiteSpace(email) ? email : $"{normalizedProvider}-{providerKey}@local.auth";
            var fallbackName = !string.IsNullOrWhiteSpace(fullName) ? fullName : $"OAuth {provider} User";

            user = new User
            {
                UserName = fallbackEmail,
                Email = fallbackEmail,
                FullName = fallbackName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Candidate");
        }

        var existingLogin = await _userManager.FindByLoginAsync(provider, providerKey);
        if (existingLogin is null)
        {
            var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider));
            if (!addLoginResult.Succeeded)
                throw new InvalidOperationException(string.Join("; ", addLoginResult.Errors.Select(e => e.Description)));
        }

        var token = GenerateJwtToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _context.RefreshTokens
            .SingleOrDefaultAsync(r => r.Token == refreshToken && r.ExpiresAt > DateTime.UtcNow);

        if (stored == null)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid or expired");
        }

        var user = await _context.Users.SingleAsync(u => u.Id == stored.UserId);

        _context.RefreshTokens.Remove(stored);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var newRefreshToken = await CreateRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await _context.RefreshTokens
            .SingleOrDefaultAsync(r => r.Token == refreshToken);

        if (stored != null)
        {
            _context.RefreshTokens.Remove(stored);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            throw new InvalidOperationException("Email does not exist");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        await _emailService.SendAsync(
            dto.Email,
            "Reset your password",
            $"Your reset token: {token}");
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            throw new InvalidOperationException("Email does not exist");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Reset token is invalid or expired");

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? "");
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters for HS256.");

        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresMinutes = int.Parse(jwtSection["ExpiresInMinutes"] ?? "60");

        var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();

        var emailValue = user.Email ?? user.UserName ?? $"{user.Id}@local";
        var displayName = user.FullName ?? user.UserName ?? emailValue ?? user.Id.ToString();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("UserId", user.Id.ToString()),
            // Ensure controllers using ClaimTypes.NameIdentifier can resolve current user id.
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, emailValue),
            new Claim(ClaimTypes.Email, emailValue),
            new Claim(ClaimTypes.Name, displayName)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateRefreshTokenAsync(User user)
    {
        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = expiresAt
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return token;
    }
}

