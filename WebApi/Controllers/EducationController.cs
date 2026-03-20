using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EducationController : ControllerBase
{
    private readonly IEducationService _educationService;

    public EducationController(IEducationService educationService)
    {
        _educationService = educationService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Response<string>> AddAsync(CreateEducationDto dto)
    {
        return await _educationService.AddAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<Education>> GetByIdAsync(int id)
    {
        return await _educationService.GetByIdAsync(id);
    }

    [HttpGet]
    [Authorize]
    public async Task<Response<List<Education>>> GetAllAsync()
    {
        return await _educationService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<string>> UpdateAsync(int id, UpdateEducationDto dto)
    {
        return await _educationService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _educationService.DeleteAsync(id);
    }

    [HttpGet("by-profile/{profileId}")]
    [Authorize]
    public async Task<Response<List<Education>>> GetByProfileIdAsync(int profileId)
    {
        return await _educationService.GetByProfileIdAsync(profileId);
    }
}
