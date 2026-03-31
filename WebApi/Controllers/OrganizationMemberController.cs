using System.Security.Claims;
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

    private int ActingUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> AddAsync(CreateOrganizationMemberDto dto)
    {
        return await _organizationMemberService.CreateAsync(dto, ActingUserId);
    }

    [HttpPost("invite")]
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> InviteAsync([FromBody] CreateOrganizationMemberDto dto)
    {
        return await _organizationMemberService.InviteAsync(dto, ActingUserId);
    }

    [HttpPut("invitation/{invitationId:int}/respond")]
    [Authorize]
    public async Task<Response<string>> RespondToInvitationAsync(
        int invitationId,
        [FromBody] OrganizationMemberInviteRespondDto dto)
    {
        return await _organizationMemberService.RespondToInvitationAsync(invitationId, ActingUserId, dto);
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
    [Authorize(Roles = "Organization")]
    public async Task<Response<string>> UpdateAsync(int id, UpdateOrganizationMemberDto dto)
    {
        return await _organizationMemberService.UpdateAsync(id, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organization")]
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
