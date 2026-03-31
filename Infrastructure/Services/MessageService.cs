using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class MessageService(ApplicationDbContext dbContext) : IMessageService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(int senderId, CreateMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return new Response<string>(HttpStatusCode.BadRequest, "Message cannot be empty");
        if (dto.Content.Length > 2000)
            return new Response<string>(HttpStatusCode.BadRequest, "Message is too long");

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
            CreatedAt = DateTime.UtcNow,
        };
        await context.Messages.AddAsync(message);
        await context.SaveChangesAsync();

        // Intentionally no in-app Notification row: new mail belongs in Messages + unread counts only.

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

        if (list.Count > 0)
        {
            var latestId = list[^1].Id;
            if (conv.User1Id == userId)
                conv.User1LastReadMessageId = latestId;
            else
                conv.User2LastReadMessageId = latestId;
            await context.SaveChangesAsync();
        }

        return new Response<List<Message>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> DeleteAsync(int messageId, int userId)
    {
        var msg = await context.Messages.FindAsync(messageId);
        if (msg == null)
            return new Response<string>(HttpStatusCode.NotFound, "Message not found");
        var conv = await context.Conversations.FindAsync(msg.ConversationId);
        if (conv == null)
            return new Response<string>(HttpStatusCode.NotFound, "Conversation not found");
        if (conv.User1Id != userId && conv.User2Id != userId)
            return new Response<string>(HttpStatusCode.Forbidden, "You are not in this conversation");

        context.Messages.Remove(msg);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok", "deleted");
    }
}
