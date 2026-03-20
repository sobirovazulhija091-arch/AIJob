using Domain.DTOs;

public interface IUserExperienceService
{
    Task<Response<string>> CreateAsync(CreateUserExperienceDto dto);
    Task<Response<UserExperience>> GetByIdAsync(int id);
    Task<Response<List<UserExperience>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateUserExperienceDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<UserExperience>>> GetByUserIdAsync(int userId);
}