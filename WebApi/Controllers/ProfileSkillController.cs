using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProfileSkillController : ControllerBase
{
    private readonly IProfileSkillService _profileSkillService;

    public ProfileSkillController(IProfileSkillService profileSkillService)
    {
        _profileSkillService = profileSkillService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Response<ProfileSkill>> AddAsync(CreateProfileSkillDto dto)
    {
        return await _profileSkillService.AddAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<ProfileSkill>> GetByIdAsync(int id)
    {
        return await _profileSkillService.GetByIdAsync(id);
    }

    [HttpGet]
    [Authorize]
    public async Task<Response<List<ProfileSkill>>> GetAllAsync()
    {
        return await _profileSkillService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<ProfileSkill>> UpdateAsync(int id, UpdateProfileSkillDto dto)
    {
        return await _profileSkillService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _profileSkillService.DeleteAsync(id);
    }

    [HttpGet("by-profile/{profileId}")]
    [Authorize]
    public async Task<Response<List<ProfileSkill>>> GetByProfileIdAsync(int profileId)
    {
        return await _profileSkillService.GetByProfileIdAsync(profileId);
    }

    [HttpDelete("profile/{profileId}/skill/{skillId}")]
    [Authorize]
    public async Task<Response<string>> RemoveSkillAsync(int profileId, int skillId)
    {
        return await _profileSkillService.RemoveSkillAsync(profileId, skillId);
    }
}
