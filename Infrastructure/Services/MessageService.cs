using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class MessageService(ApplicationDbContext dbContext, INotificationService notifications) : IMessageService
{
    private readonly ApplicationDbContext context = dbContext;
    private readonly INotificationService _notifications = notifications;

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
            CreatedAt = DateTime.UtcNow
        };
        await context.Messages.AddAsync(message);
        await context.SaveChangesAsync();

        var recipientId = conv.User1Id == senderId ? conv.User2Id : conv.User1Id;
        var preview = dto.Content.Trim();
        if (preview.Length > 200) preview = preview[..200] + "…";
        try
        {
            await _notifications.CreateAsync(new CreateNotificationDto
            {
                UserId = recipientId,
                Type = NotificationType.MessageReceived,
                Title = "New message",
                Message = preview,
            });
        }
        catch
        {
            // delivered in-app message even if notification row fails
        }

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
