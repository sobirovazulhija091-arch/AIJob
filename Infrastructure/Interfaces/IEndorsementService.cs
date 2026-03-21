using Domain.DTOs;
using Domain.Entities;

public interface IEndorsementService
{
    Task<Response<string>> AddAsync(int endorserId, CreateEndorsementDto dto);
    Task<Response<string>> RemoveAsync(int endorsementId, int userId);
    Task<Response<Endorsement>> GetByIdAsync(int id);
    Task<Response<List<Endorsement>>> GetByProfileSkillIdAsync(int profileSkillId);
    Task<Response<List<Endorsement>>> GetByUserIdAsync(int userId);
}
