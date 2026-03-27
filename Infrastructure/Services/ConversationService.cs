using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class ConversationService(ApplicationDbContext dbContext) : IConversationService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<Conversation>> GetOrCreateAsync(int userId, int otherUserId)
    {
        if (userId == otherUserId)
            return new Response<Conversation>(HttpStatusCode.BadRequest, "Cannot create conversation with yourself");

        var u1 = Math.Min(userId, otherUserId);
        var u2 = Math.Max(userId, otherUserId);

        var existing = await context.Conversations
            .FirstOrDefaultAsync(c => c.User1Id == u1 && c.User2Id == u2);
        if (existing != null)
            return new Response<Conversation>(HttpStatusCode.OK, "ok", existing);

        var conv = new Conversation
        {
            User1Id = u1,
            User2Id = u2,
            CreatedAt = DateTime.UtcNow
        };
        await context.Conversations.AddAsync(conv);
        await context.SaveChangesAsync();
        return new Response<Conversation>(HttpStatusCode.OK, "Conversation created", conv);
    }

    public async Task<Response<List<Conversation>>> GetByUserIdAsync(int userId)
    {
        var list = await context.Conversations
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return new Response<List<Conversation>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<Conversation>> GetByIdAsync(int id, int userId)
    {
        var get = await context.Conversations.FindAsync(id);
        if (get == null)
            return new Response<Conversation>(HttpStatusCode.NotFound, "Conversation not found");
        if (get.User1Id != userId && get.User2Id != userId)
            return new Response<Conversation>(HttpStatusCode.Forbidden, "You are not in this conversation");
        return new Response<Conversation>(HttpStatusCode.OK, "ok", get);
    }
}
