using Domain.DTOs;

public interface IUserEducationService
{
    Task<Response<string>> CreateAsync(CreateUserEducationDto dto);
    Task<Response<UserEducation>> GetByIdAsync(int id);
    Task<Response<List<UserEducation>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateUserEducationDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<UserEducation>>> GetByUserIdAsync(int userId);
}