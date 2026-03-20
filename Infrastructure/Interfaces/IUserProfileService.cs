using Domain.DTOs;
using Infrastructure.Responses;

public interface IUserProfileService
{
    Task<Response<string>> CreateAsync(CreateUserProfileDto dto);
    Task<Response<UserProfile>> GetByIdAsync(int id);
    Task<Response<List<UserProfile>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateUserProfileDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<UserProfile>> GetByUserIdAsync(int userId);
}