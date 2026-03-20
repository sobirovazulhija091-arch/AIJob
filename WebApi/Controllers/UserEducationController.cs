using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserEducationController : ControllerBase
{
    private readonly IUserEducationService _userEducationService;

    public UserEducationController(IUserEducationService userEducationService)
    {
        _userEducationService = userEducationService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Response<string>> AddAsync(CreateUserEducationDto dto)
    {
        return await _userEducationService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<UserEducation>> GetByIdAsync(int id)
    {
        return await _userEducationService.GetByIdAsync(id);
    }

    [HttpGet]
    [Authorize]
    public async Task<Response<List<UserEducation>>> GetAllAsync()
    {
        return await _userEducationService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<string>> UpdateAsync(int id, UpdateUserEducationDto dto)
    {
        return await _userEducationService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _userEducationService.DeleteAsync(id);
    }

    [HttpGet("by-user/{userId}")]
    [Authorize]
    public async Task<Response<List<UserEducation>>> GetByUserIdAsync(int userId)
    {
        return await _userEducationService.GetByUserIdAsync(userId);
    }
}
