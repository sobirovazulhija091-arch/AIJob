using Domain.DTOs;

public interface IProfileSkillService
{
    Task<Response<ProfileSkill>> AddAsync(CreateProfileSkillDto dto);
    Task<Response<ProfileSkill>> GetByIdAsync(int id);
    Task<Response<List<ProfileSkill>>> GetAllAsync();
    Task<Response<ProfileSkill>> UpdateAsync(int id, UpdateProfileSkillDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<ProfileSkill>>> GetByProfileIdAsync(int profileId);
    Task<Response<string>> RemoveSkillAsync(int profileId, int skillId);
}