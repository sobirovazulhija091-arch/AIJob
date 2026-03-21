using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<Response<string>> CreateAsync([FromBody] CreateRecommendationDto dto)
    {
        return await _recommendationService.CreateAsync(GetUserId(), dto);
    }

    [HttpDelete("{id}")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _recommendationService.DeleteAsync(id, GetUserId());
    }

    [HttpGet("{id}")]
    public async Task<Response<Recommendation>> GetByIdAsync(int id)
    {
        return await _recommendationService.GetByIdAsync(id);
    }

    [HttpGet("by-recipient/{recipientId}")]
    public async Task<Response<List<Recommendation>>> GetByRecipientIdAsync(int recipientId)
    {
        return await _recommendationService.GetByRecipientIdAsync(recipientId);
    }
}
