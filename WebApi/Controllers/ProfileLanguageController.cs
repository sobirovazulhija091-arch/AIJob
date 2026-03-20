using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProfileLanguageController : ControllerBase
{
    private readonly IProfileLanguageService _profileLanguageService;

    public ProfileLanguageController(IProfileLanguageService profileLanguageService)
    {
        _profileLanguageService = profileLanguageService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Response<ProfileLanguage>> AddAsync(CreateProfileLanguageDto dto)
    {
        return await _profileLanguageService.AddAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<ProfileLanguage>> GetByIdAsync(int id)
    {
        return await _profileLanguageService.GetByIdAsync(id);
    }

    [HttpGet]
    [Authorize]
    public async Task<Response<List<ProfileLanguage>>> GetAllAsync()
    {
        return await _profileLanguageService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<Response<ProfileLanguage>> UpdateAsync(int id, UpdateProfileLanguageDto dto)
    {
        return await _profileLanguageService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _profileLanguageService.DeleteAsync(id);
    }

    [HttpGet("by-profile/{profileId}")]
    [Authorize]
    public async Task<Response<List<ProfileLanguage>>> GetByProfileIdAsync(int profileId)
    {
        return await _profileLanguageService.GetByProfileIdAsync(profileId);
    }
}
