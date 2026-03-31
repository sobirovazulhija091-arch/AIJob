namespace Domain.DTOs;

public class PostFeedItemDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RepostOfPostId { get; set; }
    /// <summary>When this item is a repost, user id of the original post author.</summary>
    public int? RepostSourceUserId { get; set; }
    public int LikeCount { get; set; }
    public bool LikedByMe { get; set; }
    /// <summary>How many repost rows point at this post (<see cref="RepostOfPostId" />).</summary>
    public int RepostCount { get; set; }
}
