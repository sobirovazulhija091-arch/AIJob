using System.Net;
using System.Security.Claims;
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
    [Authorize(Roles = "Organization")]
    public async Task<Response<Organization>> AddAsync(CreateOrganizationDto dto)
    {
        return await _organizationService.CreateAsync(dto);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<List<Organization>>> GetMineAsync()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return new Response<List<Organization>>(HttpStatusCode.Unauthorized, "Invalid user");
        return await _organizationService.GetForUserAsync(userId);
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
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateOrganizationDto dto)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return new Response<string>(HttpStatusCode.Unauthorized, "Invalid user");
        return await _organizationService.UpdateAsync(id, dto, userId);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
            return new Response<string>(HttpStatusCode.Unauthorized, "Invalid user");
        return await _organizationService.DeleteAsync(id, userId);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<Response<List<Organization>>> SearchByNameAsync([FromQuery] string name)
    {
        return await _organizationService.SearchByNameAsync(name);
    }
}
