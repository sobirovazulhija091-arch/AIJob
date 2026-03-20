using Domain.DTOs;
using Infrastructure.Responses;

public interface IProfileService
{
    Task<Response<string>> CreateAsync(CreateProfileDto dto);
    Task<Response<Profile>> GetByIdAsync(int id);
    Task<Response<List<Profile>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateProfileDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<Profile>> GetByUserIdAsync(int userId);
}
