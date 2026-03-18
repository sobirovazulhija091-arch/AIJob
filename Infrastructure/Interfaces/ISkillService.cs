using Domain.DTOs;

public interface ISkillService
{
    Task<Response<SkillResponseDto>> CreateAsync(CreateSkillDto dto);
    Task<Response<SkillResponseDto>> GetByIdAsync(int id);
    Task<Response<IEnumerable<SkillResponseDto>>> GetAllAsync();
    Task<Response<SkillResponseDto>> UpdateAsync(int id, UpdateSkillResponseDto dto);
    Task<Response<SkillResponseDto>> DeleteAsync(int id);

    Task<Response<List<SkillResponseDto>>> SearchByNameAsync(string name);
}