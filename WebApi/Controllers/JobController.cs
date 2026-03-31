using System.Net;
using System.Security.Claims;
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
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> AddAsync(CreateJobDto dto)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return new Response<string>(HttpStatusCode.Unauthorized, "Invalid user");
        return await _jobService.AddAsync(dto, userId);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<List<Job>>> GetMineAsync()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return new Response<List<Job>>(HttpStatusCode.Unauthorized, "Invalid user");
        return await _jobService.GetForUserAsync(userId);
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
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateJobDto dto)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return new Response<string>(HttpStatusCode.Unauthorized, "Invalid user");
        return await _jobService.UpdateAsync(id, dto, userId);
    }
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return new Response<string>(HttpStatusCode.Unauthorized, "Invalid user");
        return await _jobService.DeleteAsync(id, userId);
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