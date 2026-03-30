using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Response<string>> AddAsync(CreateUserProfileDto dto)
    {
        return await _userProfileService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<UserProfile>> GetByIdAsync(int id)
    {
        return await _userProfileService.GetByIdAsync(id);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<Response<List<UserProfile>>> GetAllAsync()
    {
        return await _userProfileService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<string>> UpdateAsync(int id, UpdateUserProfileDto dto)
    {
        return await _userProfileService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _userProfileService.DeleteAsync(id);
    }

    [HttpGet("by-user/{userId}")]
    [Authorize]
    public async Task<Response<UserProfile>> GetByUserIdAsync(int userId)
    {
        return await _userProfileService.GetByUserIdAsync(userId);
    }

    [HttpPost("public-by-users")]
    [Authorize]
    public async Task<Response<List<UserPublicProfileDto>>> GetPublicByUsersAsync([FromBody] List<int> userIds)
    {
        return await _userProfileService.GetPublicProfilesByUserIdsAsync(userIds);
    }

    /// <summary>LinkedIn-style member view: about + experience (no salary/CV).</summary>
    [HttpGet("member/{userId:int}")]
    [Authorize]
    public async Task<Response<MemberProfileDto>> GetMemberProfile(int userId)
    {
        return await _userProfileService.GetMemberProfileAsync(userId);
    }
}
