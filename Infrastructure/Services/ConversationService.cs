using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class ConversationService(ApplicationDbContext dbContext) : IConversationService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<ConversationListItemDto>> GetOrCreateAsync(int userId, int otherUserId)
    {
        if (userId == otherUserId)
            return new Response<ConversationListItemDto>(HttpStatusCode.BadRequest, "Cannot create conversation with yourself");

        var u1 = Math.Min(userId, otherUserId);
        var u2 = Math.Max(userId, otherUserId);

        var existing = await context.Conversations
            .FirstOrDefaultAsync(c => c.User1Id == u1 && c.User2Id == u2);
        if (existing != null)
            return new Response<ConversationListItemDto>(HttpStatusCode.OK, "ok", await ToListItemAsync(existing, userId));

        var conv = new Conversation
        {
            User1Id = u1,
            User2Id = u2,
            CreatedAt = DateTime.UtcNow,
        };
        await context.Conversations.AddAsync(conv);
        await context.SaveChangesAsync();
        return new Response<ConversationListItemDto>(
            HttpStatusCode.OK,
            "Conversation created",
            await ToListItemAsync(conv, userId));
    }

    public async Task<Response<List<ConversationListItemDto>>> GetByUserIdAsync(int userId)
    {
        var list = await context.Conversations
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .ToListAsync();
        if (list.Count == 0)
            return new Response<List<ConversationListItemDto>>(HttpStatusCode.OK, "ok", []);

        var convIds = list.ConvertAll(c => c.Id);
        var allMsgs = await context.Messages
            .AsNoTracking()
            .Where(m => convIds.Contains(m.ConversationId))
            .ToListAsync();

        var items = new List<ConversationListItemDto>(list.Count);
        foreach (var c in list)
        {
            var msgs = allMsgs.Where(m => m.ConversationId == c.Id).ToList();
            var last = msgs.Count == 0
                ? null
                : msgs.OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id).First();
            int? lastRead = c.User1Id == userId ? c.User1LastReadMessageId : c.User2LastReadMessageId;
            var unread = msgs.Count(m => m.SenderId != userId && (lastRead == null || m.Id > lastRead.Value));
            items.Add(new ConversationListItemDto
            {
                Id = c.Id,
                User1Id = c.User1Id,
                User2Id = c.User2Id,
                CreatedAt = c.CreatedAt,
                UnreadCount = unread,
                LastMessagePreview = last == null ? null : TruncatePreview(last.Content),
                LastMessageAt = last?.CreatedAt,
            });
        }

        items.Sort((a, b) =>
        {
            var ta = a.LastMessageAt ?? a.CreatedAt;
            var tb = b.LastMessageAt ?? b.CreatedAt;
            return tb.CompareTo(ta);
        });

        // One row per other participant (defensive if legacy duplicates remain).
        items = items
            .GroupBy(i => i.User1Id == userId ? i.User2Id : i.User1Id)
            .Select(g =>
            {
                var ordered = g
                    .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
                    .ThenByDescending(x => x.Id)
                    .ToList();
                var best = ordered[0];
                best.UnreadCount = ordered.Sum(x => x.UnreadCount);
                return best;
            })
            .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
            .ToList();

        return new Response<List<ConversationListItemDto>>(HttpStatusCode.OK, "ok", items);
    }

    public async Task<Response<ConversationListItemDto>> GetByIdAsync(int id, int userId)
    {
        var get = await context.Conversations.FindAsync(id);
        if (get == null)
            return new Response<ConversationListItemDto>(HttpStatusCode.NotFound, "Conversation not found");
        if (get.User1Id != userId && get.User2Id != userId)
            return new Response<ConversationListItemDto>(HttpStatusCode.Forbidden, "You are not in this conversation");
        return new Response<ConversationListItemDto>(HttpStatusCode.OK, "ok", await ToListItemAsync(get, userId));
    }

    public async Task<Response<string>> DeleteAsync(int conversationId, int userId)
    {
        var conv = await context.Conversations.FindAsync(conversationId);
        if (conv == null)
            return new Response<string>(HttpStatusCode.NotFound, "Conversation not found");
        if (conv.User1Id != userId && conv.User2Id != userId)
            return new Response<string>(HttpStatusCode.Forbidden, "You are not in this conversation");

        var msgs = await context.Messages.Where(m => m.ConversationId == conversationId).ToListAsync();
        if (msgs.Count > 0)
            context.Messages.RemoveRange(msgs);
        context.Conversations.Remove(conv);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Conversation deleted");
    }

    private static string TruncatePreview(string content, int max = 72)
    {
        var s = content.Trim().ReplaceLineEndings(" ");
        if (s.Length <= max) return s;
        return s[..max] + "…";
    }

    private async Task<ConversationListItemDto> ToListItemAsync(Conversation c, int userId)
    {
        var msgs = await context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == c.Id)
            .ToListAsync();

        var last = msgs.Count == 0
            ? null
            : msgs.OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id).First();

        int? lastRead = c.User1Id == userId ? c.User1LastReadMessageId : c.User2LastReadMessageId;
        var unread = msgs.Count(m => m.SenderId != userId && (lastRead == null || m.Id > lastRead.Value));

        return new ConversationListItemDto
        {
            Id = c.Id,
            User1Id = c.User1Id,
            User2Id = c.User2Id,
            CreatedAt = c.CreatedAt,
            UnreadCount = unread,
            LastMessagePreview = last == null ? null : TruncatePreview(last.Content),
            LastMessageAt = last?.CreatedAt,
        };
    }
}
