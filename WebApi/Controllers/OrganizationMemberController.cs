using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrganizationMemberController : ControllerBase
{
    private readonly IOrganizationMemberService _organizationMemberService;

    public OrganizationMemberController(IOrganizationMemberService organizationMemberService)
    {
        _organizationMemberService = organizationMemberService;
    }

    [HttpPost]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> AddAsync(CreateOrganizationMemberDto dto)
    {
        return await _organizationMemberService.CreateAsync(dto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<OrganizationMember>> GetByIdAsync(int id)
    {
        return await _organizationMemberService.GetByIdAsync(id);
    }

    [HttpGet]
    [Authorize]
    public async Task<Response<List<OrganizationMember>>> GetAllAsync()
    {
        return await _organizationMemberService.GetAllAsync();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateOrganizationMemberDto dto)
    {
        return await _organizationMemberService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organization,Admin")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _organizationMemberService.DeleteAsync(id);
    }

    [HttpGet("by-organization/{organizationId}")]
    [Authorize]
    public async Task<Response<List<OrganizationMember>>> GetByOrganizationIdAsync(int organizationId)
    {
        return await _organizationMemberService.GetByOrganizationIdAsync(organizationId);
    }
}
