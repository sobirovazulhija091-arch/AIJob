using System.Security.Claims;
using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserSettingsController : ControllerBase
{
    private readonly IUserSettingsService _userSettingsService;

    public UserSettingsController(IUserSettingsService userSettingsService)
    {
        _userSettingsService = userSettingsService;
    }

    [HttpGet("me")]
    public async Task<Response<UserSettingsDto>> GetMySettingsAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _userSettingsService.GetByUserIdAsync(userId);
    }

    [HttpPut("me")]
    public async Task<Response<string>> UpdateMySettingsAsync([FromBody] UpdateUserSettingsDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _userSettingsService.UpdateByUserIdAsync(userId, dto);
    }
}
