using System.Net;
using System.Security.Claims;
using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class JobApplicationController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;

    public JobApplicationController(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService;
    }

    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<Response<string>> AddAsync(CreateJobApplicationDto dto)
    {
        return await _jobApplicationService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<JobApplication>> GetByIdAsync(int id)
    {
        return await _jobApplicationService.GetByIdAsync(id);
    }

    [HttpGet("paged")]
    [Authorize]
    public async Task<PagedResult<JobApplication>> GetPagedAsync([FromQuery] JobApplicationFilter filter, [FromQuery] PagedQuery querypage)
    {
        return await _jobApplicationService.GetPagedAsync(filter, querypage);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateJobApplicationDto dto)
    {
        return await _jobApplicationService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Candidate,Organization")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _jobApplicationService.DeleteAsync(id);
    }

    [HttpGet("by-user/{userId}")]
    [Authorize]
    public async Task<Response<List<JobApplication>>> GetByUserIdAsync(int userId)
    {
        return await _jobApplicationService.GetByUserIdAsync(userId);
    }

    [HttpGet("by-job/{jobId}")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<List<JobApplication>>> GetByJobIdAsync(int jobId)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return new Response<List<JobApplication>>(HttpStatusCode.Unauthorized, "Invalid user");
        return await _jobApplicationService.GetByJobIdAsync(jobId, userId);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> ChangeStatusAsync(int id, [FromBody] ApplicationStatus status)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return new Response<string>(HttpStatusCode.Unauthorized, "Invalid user");
        return await _jobApplicationService.ChangeStatusAsync(id, status, userId);
    }
}
