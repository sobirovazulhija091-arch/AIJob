using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Response<string>> AddAsync(CreateProfileDto dto)
    {
        return await _profileService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<Profile>> GetByIdAsync(int id)
    {
        return await _profileService.GetByIdAsync(id);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<string>> UpdateAsync(int id, UpdateProfileDto dto)
    {
        return await _profileService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _profileService.DeleteAsync(id);
    }

    [HttpGet("by-user/{userId}")]
    [Authorize]
    public async Task<Response<Profile>> GetByUserIdAsync(int userId)
    {
        return await _profileService.GetByUserIdAsync(userId);
    }
}
