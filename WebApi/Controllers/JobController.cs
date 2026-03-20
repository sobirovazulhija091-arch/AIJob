using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> AddAsync(CreateJobDto dto)
    {
        return await _jobService.AddAsync(dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<Response<Job>> GetByIdAsync(int id)
    {
        return await _jobService.GetByIdAsync(id);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<Response<List<Job>>> GetAllAsync()
    {        
        return await _jobService.GetAllAsync();
    }
    [HttpGet("paged")]
    [AllowAnonymous]
    public async Task<PagedResult<Job>> GetPagedAsync([FromQuery] JobFilter filter,[FromQuery] PagedQuery querypage)
    {
         return await _jobService.GetPagedAsync(filter,querypage);
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateJobDto dto)
    {
        return await _jobService.UpdateAsync(id,dto);
    }
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _jobService.DeleteAsync(id);
    }
    [HttpGet("by-organization/{organizationId}")]
    [AllowAnonymous]
    public async Task<Response<List<Job>>> GetByOrganizationIdAsync(int organizationId)
    {
        return await _jobService.GetByOrganizationIdAsync(organizationId);
    }
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<Response<List<Job>>> SearchByTitleAsync([FromQuery] string title)
    {
        return await _jobService.SearchByTitleAsync(title);
    }
}