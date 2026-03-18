using Domain.DTOs;

public interface IUserEducationService
{
    Task<Response<UserEducationResponseDto>> CreateAsync(CreateUserEducationDto dto);
    Task<Response<UserEducationResponseDto>> GetByIdAsync(int id);
    Task<Response<IEnumerable<UserEducationResponseDto>>> GetAllAsync();
    Task<Response<UserEducationResponseDto>> UpdateAsync(int id, UpdateUserEducationDto dto);
    Task<Response<UserEducationResponseDto>> DeleteAsync(int id);
    Task<Response<List<UserEducationResponseDto>>> GetByUserIdAsync(int userId);
}