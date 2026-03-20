using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class MessageService(ApplicationDbContext dbContext) : IMessageService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(int senderId, CreateMessageDto dto)
    {
        var conv = await context.Conversations.FindAsync(dto.ConversationId);
        if (conv == null)
            return new Response<string>(HttpStatusCode.NotFound, "Conversation not found");
        if (conv.User1Id != senderId && conv.User2Id != senderId)
            return new Response<string>(HttpStatusCode.Forbidden, "You are not in this conversation");

        var message = new Message
        {
            ConversationId = dto.ConversationId,
            SenderId = senderId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };
        await context.Messages.AddAsync(message);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Message sent");
    }

    public async Task<Response<Message>> GetByIdAsync(int id)
    {
        var get = await context.Messages.FindAsync(id);
        if (get == null)
            return new Response<Message>(HttpStatusCode.NotFound, "Message not found");
        return new Response<Message>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<Message>>> GetByConversationIdAsync(int conversationId, int userId)
    {
        var conv = await context.Conversations.FindAsync(conversationId);
        if (conv == null)
            return new Response<List<Message>>(HttpStatusCode.NotFound, "Conversation not found");
        if (conv.User1Id != userId && conv.User2Id != userId)
            return new Response<List<Message>>(HttpStatusCode.Forbidden, "You are not in this conversation");

        var list = await context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
        return new Response<List<Message>>(HttpStatusCode.OK, "ok", list);
    }
}
