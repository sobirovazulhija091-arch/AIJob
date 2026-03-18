using Domain.DTOs;
using Infrastructure.Responses;

public interface IUserSkillService
{
    Task<Response<UserSkillResponseDto>> CreateAsync(CreateUserSkillDto dto);
    Task<Response<UserSkillResponseDto>> GetByIdAsync(int id);
    Task<Response<IEnumerable<UserSkillResponseDto>>> GetAllAsync();
    Task<Response<UserSkillResponseDto>> UpdateAsync(int id, UpdateUserSkillDto dto);
    Task<Response<UserSkillResponseDto>> DeleteAsync(int id);

    Task<Response<List<SkillResponseDto>>> GetSkillsByUserIdAsync(int userId);
    Task<Response<bool>> RemoveSkillFromUserAsync(int userId, int skillId);
}