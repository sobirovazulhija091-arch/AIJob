using Domain.DTOs;

public interface IJobSkillService
{
    Task<Response<JobSkillResponseDto>> CreateAsync(CreateJobSkillDto dto);
    Task<Response<JobSkillResponseDto>> GetByIdAsync(int id);
    Task<Response<List<JobSkillResponseDto>>> GetAllAsync();
    Task<Response<JobSkillResponseDto>> UpdateAsync(int id, UpdateJobSkillDto dto);
    Task<Response<JobSkillResponseDto>> DeleteAsync(int id);

    Task<Response<List<SkillResponseDto>>> GetSkillsByJobIdAsync(int jobId);
}