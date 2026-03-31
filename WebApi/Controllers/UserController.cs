using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        return Ok(new { userId, email });
    }

    /// <summary>Minimal user rows for member directory (any authenticated user).</summary>
    [HttpGet("directory")]
    [Authorize]
    public async Task<Response<List<MemberDirectoryEntryDto>>> GetDirectoryAsync()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? excludeUserId = int.TryParse(idClaim, out var uid) ? uid : null;
        return await _userService.GetMemberDirectoryAsync(excludeUserId);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<string>> UpdateAsync(int id, UpdateUserDto dto)
    {
        return await _userService.UpdateAsync(id, dto);
    }
}
