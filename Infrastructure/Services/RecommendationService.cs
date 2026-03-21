using System.Net;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class RecommendationService(ApplicationDbContext dbContext) : IRecommendationService
{
    private readonly ApplicationDbContext context = dbContext;

    private static async Task<bool> AreConnectedAsync(ApplicationDbContext ctx, int userId1, int userId2)
    {
        return await ctx.Connections.AnyAsync(c =>
            ((c.RequesterId == userId1 && c.AddresseeId == userId2) ||
             (c.RequesterId == userId2 && c.AddresseeId == userId1)) &&
            c.Status == ConnectionStatus.Accepted);
    }

    public async Task<Response<string>> CreateAsync(int authorId, CreateRecommendationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return new Response<string>(HttpStatusCode.BadRequest, "Content is required");

        if (authorId == dto.RecipientId)
            return new Response<string>(HttpStatusCode.BadRequest, "Cannot recommend yourself");

        var recipientExists = await context.Users.AnyAsync(u => u.Id == dto.RecipientId);
        if (!recipientExists)
            return new Response<string>(HttpStatusCode.NotFound, "Recipient not found");

        var connected = await AreConnectedAsync(context, authorId, dto.RecipientId);
        if (!connected)
            return new Response<string>(HttpStatusCode.Forbidden, "Must be connected to recommend");

        var recommendation = new Recommendation
        {
            AuthorId = authorId,
            RecipientId = dto.RecipientId,
            Content = dto.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        await context.Recommendations.AddAsync(recommendation);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Recommendation created");
    }

    public async Task<Response<string>> DeleteAsync(int id, int userId)
    {
        var rec = await context.Recommendations.FindAsync(id);
        if (rec == null)
            return new Response<string>(HttpStatusCode.NotFound, "Recommendation not found");
        if (rec.AuthorId != userId && rec.RecipientId != userId)
            return new Response<string>(HttpStatusCode.Forbidden, "Not your recommendation");

        context.Recommendations.Remove(rec);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Recommendation deleted");
    }

    public async Task<Response<Recommendation>> GetByIdAsync(int id)
    {
        var rec = await context.Recommendations.FindAsync(id);
        if (rec == null)
            return new Response<Recommendation>(HttpStatusCode.NotFound, "Recommendation not found");
        return new Response<Recommendation>(HttpStatusCode.OK, "ok", rec);
    }

    public async Task<Response<List<Recommendation>>> GetByRecipientIdAsync(int recipientId)
    {
        var list = await context.Recommendations
            .Where(r => r.RecipientId == recipientId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return new Response<List<Recommendation>>(HttpStatusCode.OK, "ok", list);
    }
}
