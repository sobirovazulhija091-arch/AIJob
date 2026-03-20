using Domain.DTOs;

public interface IPostService
{
    Task<Response<string>> CreateAsync(int userId, CreatePostDto dto);
    Task<Response<Post>> GetByIdAsync(int id);
    Task<Response<List<Post>>> GetAllAsync();
    Task<Response<List<Post>>> GetFeedAsync(int userId);
    Task<Response<string>> UpdateAsync(int id, int userId, UpdatePostDto dto);
    Task<Response<string>> DeleteAsync(int id, int userId);
}
