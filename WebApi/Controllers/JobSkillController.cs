using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class JobSkillController : ControllerBase
{
    private readonly IJobSkillService _jobSkillService;

    public JobSkillController(IJobSkillService jobSkillService)
    {
        _jobSkillService = jobSkillService;
    }

    [HttpPost]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> AddAsync(CreateJobSkillDto dto)
    {
        return await _jobSkillService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<Response<JobSkill>> GetByIdAsync(int id)
    {
        return await _jobSkillService.GetByIdAsync(id);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<Response<List<JobSkill>>> GetAllAsync()
    {
        return await _jobSkillService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateJobSkillDto dto)
    {
        return await _jobSkillService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _jobSkillService.DeleteAsync(id);
    }

    [HttpGet("by-job/{jobId}")]
    [AllowAnonymous]
    public async Task<Response<List<Skill>>> GetSkillsByJobIdAsync(int jobId)
    {
        return await _jobSkillService.GetSkillsByJobIdAsync(jobId);
    }
}
