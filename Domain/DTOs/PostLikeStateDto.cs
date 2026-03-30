namespace Domain.DTOs;

public class PostLikeStateDto
{
    public int PostId { get; set; }
    public int LikeCount { get; set; }
    public bool LikedByMe { get; set; }
}
