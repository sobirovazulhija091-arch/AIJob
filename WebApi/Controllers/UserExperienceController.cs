using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserExperienceController : ControllerBase
{
    private readonly IUserExperienceService _userExperienceService;

    public UserExperienceController(IUserExperienceService userExperienceService)
    {
        _userExperienceService = userExperienceService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Response<string>> AddAsync(CreateUserExperienceDto dto)
    {
        return await _userExperienceService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<UserExperience>> GetByIdAsync(int id)
    {
        return await _userExperienceService.GetByIdAsync(id);
    }

    [HttpGet]
    [Authorize]
    public async Task<Response<List<UserExperience>>> GetAllAsync()
    {
        return await _userExperienceService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<string>> UpdateAsync(int id, UpdateUserExperienceDto dto)
    {
        return await _userExperienceService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _userExperienceService.DeleteAsync(id);
    }

    [HttpGet("by-user/{userId}")]
    [Authorize]
    public async Task<Response<List<UserExperience>>> GetByUserIdAsync(int userId)
    {
        return await _userExperienceService.GetByUserIdAsync(userId);
    }
}
