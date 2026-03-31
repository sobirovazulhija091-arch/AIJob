using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpPost]
    [Authorize(Roles = "Candidate,Organization")]
    public async Task<Response<string>> AddAsync(CreateSkillDto dto)
    {
        return await _skillService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<Response<Skill>> GetByIdAsync(int id)
    {
        return await _skillService.GetByIdAsync(id);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<Response<List<Skill>>> GetAllAsync()
    {
        return await _skillService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Candidate,Organization")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateSkillResponseDto dto)
    {
        return await _skillService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Candidate,Organization")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _skillService.DeleteAsync(id);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<Response<List<Skill>>> SearchByNameAsync([FromQuery] string name)
    {
        return await _skillService.SearchByNameAsync(name);
    }
}
