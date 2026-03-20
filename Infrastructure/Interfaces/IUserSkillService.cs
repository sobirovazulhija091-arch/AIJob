using Domain.DTOs;
using Infrastructure.Responses;

public interface IUserSkillService
{
    Task<Response<string>> CreateAsync(CreateUserSkillDto dto);
    Task<Response<UserSkill>> GetByIdAsync(int id);
    Task<Response<List<UserSkill>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateUserSkillDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<Skill>>> GetSkillsByUserIdAsync(int userId);
    Task<Response<string>> RemoveSkillFromUserAsync(int userId, int skillId);
}