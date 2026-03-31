using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class JobCategoryController : ControllerBase
{
    private readonly IJobCategoryService _jobCategoryService;

    public JobCategoryController(IJobCategoryService jobCategoryService)
    {
        _jobCategoryService = jobCategoryService;
    }

    [HttpPost]
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> AddAsync(CreateJobCategoryDto dto)
    {
        return await _jobCategoryService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<Response<JobCategory>> GetByIdAsync(int id)
    {
        return await _jobCategoryService.GetByIdAsync(id);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<Response<List<JobCategory>>> GetAllAsync()
    {
        return await _jobCategoryService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateJobCategoryDto dto)
    {
        return await _jobCategoryService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _jobCategoryService.DeleteAsync(id);
    }
}
