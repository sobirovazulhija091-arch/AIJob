using Domain.DTOs;
using Infrastructure.Responses;

public interface IUserProfileService
{
    Task<Response<UserProfileDto>> CreateAsync(CreateUserProfileDto dto);
    Task<Response<UserProfileDto>> GetByIdAsync(int id);
    Task<Response<List<UserProfileDto>>> GetAllAsync();
    Task<Response<UserProfileDto>> UpdateAsync(int id, UpdateUserProfileDto dto);
    Task<Response<bool>> DeleteAsync(int id);

    Task<Response<UserProfileDto>> GetByUserIdAsync(int userId);
}