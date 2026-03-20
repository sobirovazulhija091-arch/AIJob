using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<Response<string>> AddAsync(CreateOrganizationDto dto)
    {
        return await _organizationService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<Response<Organization>> GetByIdAsync(int id)
    {
        return await _organizationService.GetByIdAsync(id);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<Response<List<Organization>>> GetAllAsync()
    {
        return await _organizationService.GetAllAsync();
    }

    [HttpGet("paged")]
    [AllowAnonymous]
    public async Task<PagedResult<Organization>> GetPagedAsync([FromQuery] OrganizationFilter filter, [FromQuery] PagedQuery querypage)
    {
        return await _organizationService.GetPagedAsync(filter, querypage);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateOrganizationDto dto)
    {
        return await _organizationService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _organizationService.DeleteAsync(id);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<Response<List<Organization>>> SearchByNameAsync([FromQuery] string name)
    {
        return await _organizationService.SearchByNameAsync(name);
    }
}
