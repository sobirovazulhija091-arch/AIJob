using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class LanguageController : ControllerBase
{
    private readonly ILanguageService _languageService;

    public LanguageController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    [HttpPost]
    [Authorize(Roles = "Candidate,Organization")]
    public async Task<Response<string>> AddAsync(CreateLanguageDto dto)
    {
        return await _languageService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<Response<Language>> GetByIdAsync(int id)
    {
        return await _languageService.GetByIdAsync(id);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<Response<List<Language>>> GetAllAsync()
    {
        return await _languageService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Candidate,Organization")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateLanguageDto dto)
    {
        return await _languageService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Candidate,Organization")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _languageService.DeleteAsync(id);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<Response<List<Language>>> SearchByNameAsync([FromQuery] string name)
    {
        return await _languageService.SearchByNameAsync(name);
    }
}
