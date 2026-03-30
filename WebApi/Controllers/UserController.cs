using Domain.DTOs;
using Domain.Filters;
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<Response<string>> AddAsync(CreateUserDto dto)
    {
        return await _userService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<Response<UserResponseDto>> GetByIdAsync(int id)
    {
        return await _userService.GetByIdAsync(id);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<Response<List<UserResponseDto>>> GetAllAsync()
    {
        return await _userService.GetAllAsync();
    }

    [HttpGet("paged")]
    [Authorize(Roles = "Admin")]
    public async Task<PagedResult<UserResponseDto>> GetPagedAsync([FromQuery] UserFilter filter, [FromQuery] PagedQuery querypage)
    {
        return await _userService.GetPagedAsync(filter, querypage);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<string>> UpdateAsync(int id, UpdateUserDto dto)
    {
        return await _userService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _userService.DeleteAsync(id);
    }

    [HttpGet("by-email")]
    [Authorize(Roles = "Admin")]
    public async Task<Response<UserResponseDto>> GetByEmailAsync([FromQuery] string email)
    {
        return await _userService.GetByEmailAsync(email);
    }

    [HttpPatch("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<Response<string>> ChangeRoleAsync(int id, [FromBody] UserRole role)
    {
        return await _userService.ChangeRoleAsync(id, role);
    }
}
