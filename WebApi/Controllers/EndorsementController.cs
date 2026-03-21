using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EndorsementController : ControllerBase
{
    private readonly IEndorsementService _endorsementService;

    public EndorsementController(IEndorsementService endorsementService)
    {
        _endorsementService = endorsementService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<Response<string>> AddAsync([FromBody] CreateEndorsementDto dto)
    {
        return await _endorsementService.AddAsync(GetUserId(), dto);
    }

    [HttpDelete("{id}")]
    public async Task<Response<string>> RemoveAsync(int id)
    {
        return await _endorsementService.RemoveAsync(id, GetUserId());
    }

    [HttpGet("{id}")]
    public async Task<Response<Endorsement>> GetByIdAsync(int id)
    {
        return await _endorsementService.GetByIdAsync(id);
    }

    [HttpGet("by-profile-skill/{profileSkillId}")]
    public async Task<Response<List<Endorsement>>> GetByProfileSkillIdAsync(int profileSkillId)
    {
        return await _endorsementService.GetByProfileSkillIdAsync(profileSkillId);
    }

    [HttpGet("by-user/{userId}")]
    public async Task<Response<List<Endorsement>>> GetByUserIdAsync(int userId)
    {
        return await _endorsementService.GetByUserIdAsync(userId);
    }
}
