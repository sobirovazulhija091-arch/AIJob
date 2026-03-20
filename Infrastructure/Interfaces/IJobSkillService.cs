using Domain.DTOs;

public interface IJobSkillService
{
    Task<Response<string>> CreateAsync(CreateJobSkillDto dto);
    Task<Response<JobSkill>> GetByIdAsync(int id);
    Task<Response<List<JobSkill>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateJobSkillDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<Skill>>> GetSkillsByJobIdAsync(int jobId);
}