using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class PostService(ApplicationDbContext dbContext) : IPostService
{
    private readonly ApplicationDbContext context = dbContext;

    public async Task<Response<string>> CreateAsync(int userId, CreatePostDto dto)
    {
        var post = new Post
        {
            UserId = userId,
            Content = dto.Content,
            ImageUrl = dto.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };
        await context.Posts.AddAsync(post);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Post created");
    }

    public async Task<Response<Post>> GetByIdAsync(int id)
    {
        var get = await context.Posts.FindAsync(id);
        if (get == null)
            return new Response<Post>(HttpStatusCode.NotFound, "Post not found");
        return new Response<Post>(HttpStatusCode.OK, "ok", get);
    }

    public async Task<Response<List<Post>>> GetAllAsync()
    {
        var list = await context.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return new Response<List<Post>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<List<Post>>> GetFeedAsync(int userId)
    {
        var connectionIds = await context.Connections
            .Where(c => (c.RequesterId == userId || c.AddresseeId == userId) && c.Status == ConnectionStatus.Accepted)
            .SelectMany(c => new[] { c.RequesterId, c.AddresseeId })
            .Where(id => id != userId)
            .Distinct()
            .ToListAsync();
        connectionIds.Add(userId);

        var list = await context.Posts
            .Where(p => connectionIds.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return new Response<List<Post>>(HttpStatusCode.OK, "ok", list);
    }

    public async Task<Response<string>> UpdateAsync(int id, int userId, UpdatePostDto dto)
    {
        var post = await context.Posts.FindAsync(id);
        if (post == null)
            return new Response<string>(HttpStatusCode.NotFound, "Post not found");
        if (post.UserId != userId)
            return new Response<string>(HttpStatusCode.Forbidden, "Not your post");

        post.Content = dto.Content;
        post.ImageUrl = dto.ImageUrl;
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
    }

    public async Task<Response<string>> DeleteAsync(int id, int userId)
    {
        var post = await context.Posts.FindAsync(id);
        if (post == null)
            return new Response<string>(HttpStatusCode.NotFound, "Post not found");
        if (post.UserId != userId)
            return new Response<string>(HttpStatusCode.Forbidden, "Not your post");

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "Post deleted");
    }
}
