using Domain.DTOs;

public interface ISkillService
{
    Task<Response<string>> CreateAsync(CreateSkillDto dto);
    Task<Response<Skill>> GetByIdAsync(int id);
    Task<Response<List<Skill>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateSkillResponseDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<Skill>>> SearchByNameAsync(string name);
}