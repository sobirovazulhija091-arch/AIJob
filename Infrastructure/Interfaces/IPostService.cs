using Domain.DTOs;

public interface IPostService
{
    Task<Response<string>> CreateAsync(int userId, CreatePostDto dto);
    Task<Response<Post>> GetByIdAsync(int id);
    Task<Response<List<Post>>> GetAllAsync();
    Task<Response<List<PostFeedItemDto>>> GetFeedAsync(int userId);
    Task<Response<PostLikeStateDto>> ToggleLikeAsync(int postId, int userId);
    Task<Response<string>> RepostAsync(int postId, int userId);
    Task<Response<string>> UpdateAsync(int id, int userId, UpdatePostDto dto);
    Task<Response<string>> DeleteAsync(int id, int userId);
    Task<Response<List<PostCommentDto>>> GetCommentsAsync(int postId, int readerUserId);
    Task<Response<PostCommentDto>> AddCommentAsync(int postId, int userId, CreatePostCommentDto dto);
}
