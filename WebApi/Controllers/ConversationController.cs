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
    public async Task<Response<ConversationListItemDto>> GetOrCreateAsync([FromBody] CreateConversationDto dto)
    {
        return await _conversationService.GetOrCreateAsync(GetUserId(), dto.OtherUserId);
    }

    [HttpGet]
    public async Task<Response<List<ConversationListItemDto>>> GetMyConversationsAsync()
    {
        return await _conversationService.GetByUserIdAsync(GetUserId());
    }

    [HttpGet("{id}")]
    public async Task<Response<ConversationListItemDto>> GetByIdAsync(int id)
    {
        return await _conversationService.GetByIdAsync(id, GetUserId());
    }

    [HttpDelete("{id}")]
    public async Task<Response<string>> DeleteAsync(int id)
    {
        return await _conversationService.DeleteAsync(id, GetUserId());
    }

    /** POST fallback — some proxies or clients mishandle DELETE; same authorization rules apply. */
    [HttpPost("{id}/delete")]
    public async Task<Response<string>> DeleteByPostAsync(int id)
    {
        return await _conversationService.DeleteAsync(id, GetUserId());
    }
}
