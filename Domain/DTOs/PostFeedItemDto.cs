namespace Domain.DTOs;

public class PostFeedItemDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RepostOfPostId { get; set; }
    public int LikeCount { get; set; }
    public bool LikedByMe { get; set; }
}
