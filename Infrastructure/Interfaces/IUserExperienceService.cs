using Domain.DTOs;

public interface IUserExperienceService
{
    Task<Response<UserExperienceResponseDto>> CreateAsync(CreateUserExperienceDto dto);
    Task<Response<UserExperienceResponseDto>> GetByIdAsync(int id);
    Task<Response<IEnumerable<UserExperienceResponseDto>>> GetAllAsync();
    Task<Response<UserExperienceResponseDto>> UpdateAsync(int id, UpdateUserExperienceDto dto);
    Task<Response<UserExperienceResponseDto>> DeleteAsync(int id);
    Task<Response<List<UserExperienceResponseDto>>> GetByUserIdAsync(int userId);
}