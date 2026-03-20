using Domain.DTOs;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<Response<string>> CreateAsync([FromBody] CreateMessageDto dto)
    {
        return await _messageService.CreateAsync(GetUserId(), dto);
    }

    [HttpGet("{id}")]
    public async Task<Response<Message>> GetByIdAsync(int id)
    {
        return await _messageService.GetByIdAsync(id);
    }

    [HttpGet("by-conversation/{conversationId}")]
    public async Task<Response<List<Message>>> GetByConversationIdAsync(int conversationId)
    {
        return await _messageService.GetByConversationIdAsync(conversationId, GetUserId());
    }
}
