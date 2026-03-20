using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserSkillController : ControllerBase
{
    private readonly IUserSkillService _userSkillService;

    public UserSkillController(IUserSkillService userSkillService)
    {
        _userSkillService = userSkillService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Response<string>> AddAsync(CreateUserSkillDto dto)
    {
        return await _userSkillService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<UserSkill>> GetByIdAsync(int id)
    {
        return await _userSkillService.GetByIdAsync(id);
    }

    [HttpGet]
    [Authorize]
    public async Task<Response<List<UserSkill>>> GetAllAsync()
    {
        return await _userSkillService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<string>> UpdateAsync(int id, UpdateUserSkillDto dto)
    {
        return await _userSkillService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _userSkillService.DeleteAsync(id);
    }

    [HttpGet("by-user/{userId}")]
    [Authorize]
    public async Task<Response<List<Skill>>> GetSkillsByUserIdAsync(int userId)
    {
        return await _userSkillService.GetSkillsByUserIdAsync(userId);
    }

    [HttpDelete("user/{userId}/skill/{skillId}")]
    [Authorize]
    public async Task<Response<string>> RemoveSkillFromUserAsync(int userId, int skillId)
    {
        return await _userSkillService.RemoveSkillFromUserAsync(userId, skillId);
    }
}
