using System.Net;
using Domain.DTOs;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Responses;
using Microsoft.EntityFrameworkCore;

public class PostService(ApplicationDbContext dbContext, INotificationService notifications) : IPostService
{
    private readonly ApplicationDbContext context = dbContext;
    private readonly INotificationService _notifications = notifications;

    private async Task<bool> CanUserViewPostAsync(int readerUserId, Post post)
    {
        var connections = await context.Connections
            .Where(c => (c.RequesterId == readerUserId || c.AddresseeId == readerUserId) && c.Status == ConnectionStatus.Accepted)
            .Select(c => new { c.RequesterId, c.AddresseeId })
            .ToListAsync();

        var connectionIds = connections
            .SelectMany(c => new[] { c.RequesterId, c.AddresseeId })
            .Where(id => id != readerUserId)
            .Distinct()
            .Append(readerUserId)
            .ToList();

        return connectionIds.Contains(post.UserId);
    }

    private static PostCommentDto ToCommentDto(PostComment c) =>
        new()
        {
            Id = c.Id,
            PostId = c.PostId,
            UserId = c.UserId,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
        };

    private static PostFeedItemDto ToFeedItem(Post p, int likeCount, bool likedByMe) =>
        new()
        {
            Id = p.Id,
            UserId = p.UserId,
            Content = p.Content,
            ImageUrl = p.ImageUrl,
            CreatedAt = p.CreatedAt,
            RepostOfPostId = p.RepostOfPostId,
            LikeCount = likeCount,
            LikedByMe = likedByMe,
        };

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

    public async Task<Response<List<PostFeedItemDto>>> GetFeedAsync(int userId)
    {
        var connections = await context.Connections
            .Where(c => (c.RequesterId == userId || c.AddresseeId == userId) && c.Status == ConnectionStatus.Accepted)
            .Select(c => new { c.RequesterId, c.AddresseeId })
            .ToListAsync();

        var connectionIds = connections
            .SelectMany(c => new[] { c.RequesterId, c.AddresseeId })
            .Where(id => id != userId)
            .Distinct()
            .Append(userId)
            .ToList();

        var list = await context.Posts
            .Where(p => connectionIds.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        if (list.Count == 0)
            return new Response<List<PostFeedItemDto>>(HttpStatusCode.OK, "ok", []);

        var postIds = list.ConvertAll(p => p.Id);
        var countRows = await context.PostLikes
            .Where(l => postIds.Contains(l.PostId))
            .GroupBy(l => l.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToListAsync();
        var countMap = countRows.ToDictionary(x => x.PostId, x => x.Count);
        var myLikePostIds = await context.PostLikes
            .Where(l => postIds.Contains(l.PostId) && l.UserId == userId)
            .Select(l => l.PostId)
            .ToListAsync();
        var mySet = myLikePostIds.ToHashSet();

        var dto = list.ConvertAll(p => ToFeedItem(p, countMap.GetValueOrDefault(p.Id), mySet.Contains(p.Id)));
        return new Response<List<PostFeedItemDto>>(HttpStatusCode.OK, "ok", dto);
    }

    public async Task<Response<PostLikeStateDto>> ToggleLikeAsync(int postId, int userId)
    {
        var post = await context.Posts.FindAsync(postId);
        if (post == null)
            return new Response<PostLikeStateDto>(HttpStatusCode.NotFound, "Post not found");
        if (!await CanUserViewPostAsync(userId, post))
            return new Response<PostLikeStateDto>(HttpStatusCode.Forbidden, "You cannot like this post");

        var existing = await context.PostLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        if (existing != null)
            context.PostLikes.Remove(existing);
        else
            await context.PostLikes.AddAsync(new PostLike { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow });

        await context.SaveChangesAsync();

        var count = await context.PostLikes.CountAsync(l => l.PostId == postId);
        var likedByMe = existing == null;
        return new Response<PostLikeStateDto>(
            HttpStatusCode.OK,
            "ok",
            new PostLikeStateDto { PostId = postId, LikeCount = count, LikedByMe = likedByMe });
    }

    public async Task<Response<string>> RepostAsync(int postId, int userId)
    {
        var original = await context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == postId);
        if (original == null)
            return new Response<string>(HttpStatusCode.NotFound, "Post not found");
        if (!await CanUserViewPostAsync(userId, original))
            return new Response<string>(HttpStatusCode.Forbidden, "You cannot repost this");

        var copy = new Post
        {
            UserId = userId,
            Content = original.Content,
            ImageUrl = original.ImageUrl,
            RepostOfPostId = postId,
            CreatedAt = DateTime.UtcNow,
        };
        await context.Posts.AddAsync(copy);
        await context.SaveChangesAsync();
        return new Response<string>(HttpStatusCode.OK, "ok");
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

    public async Task<Response<List<PostCommentDto>>> GetCommentsAsync(int postId, int readerUserId)
    {
        var post = await context.Posts.FindAsync(postId);
        if (post == null)
            return new Response<List<PostCommentDto>>(HttpStatusCode.NotFound, "Post not found");
        if (!await CanUserViewPostAsync(readerUserId, post))
            return new Response<List<PostCommentDto>>(HttpStatusCode.Forbidden, "You cannot view this post");

        var list = await context.PostComments
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return new Response<List<PostCommentDto>>(
            HttpStatusCode.OK,
            "ok",
            list.ConvertAll(ToCommentDto));
    }

    public async Task<Response<PostCommentDto>> AddCommentAsync(int postId, int userId, CreatePostCommentDto dto)
    {
        var content = dto.Content?.Trim() ?? "";
        if (content.Length == 0)
            return new Response<PostCommentDto>(HttpStatusCode.BadRequest, "Comment cannot be empty");
        if (content.Length > 2000)
            return new Response<PostCommentDto>(HttpStatusCode.BadRequest, "Comment is too long");

        var post = await context.Posts.FindAsync(postId);
        if (post == null)
            return new Response<PostCommentDto>(HttpStatusCode.NotFound, "Post not found");
        if (!await CanUserViewPostAsync(userId, post))
            return new Response<PostCommentDto>(HttpStatusCode.Forbidden, "You cannot comment on this post");

        var row = new PostComment
        {
            PostId = postId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
        };
        await context.PostComments.AddAsync(row);
        await context.SaveChangesAsync();

        if (post.UserId != userId)
        {
            try
            {
                var snippet = content.Length > 120 ? content[..120] + "…" : content;
                await _notifications.CreateAsync(new CreateNotificationDto
                {
                    UserId = post.UserId,
                    Type = NotificationType.PostCommented,
                    Title = "New comment on your post",
                    Message = snippet,
                });
            }
            catch
            {
            }
        }

        return new Response<PostCommentDto>(HttpStatusCode.OK, "ok", ToCommentDto(row));
    }
}
