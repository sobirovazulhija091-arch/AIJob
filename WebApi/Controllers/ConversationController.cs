using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public ConversationController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<Response<Conversation>> GetOrCreateAsync([FromBody] CreateConversationDto dto)
    {
        return await _conversationService.GetOrCreateAsync(GetUserId(), dto.OtherUserId);
    }

    [HttpGet]
    public async Task<Response<List<Conversation>>> GetMyConversationsAsync()
    {
        return await _conversationService.GetByUserIdAsync(GetUserId());
    }

    [HttpGet("{id}")]
    public async Task<Response<Conversation>> GetByIdAsync(int id)
    {
        return await _conversationService.GetByIdAsync(id, GetUserId());
    }
}
