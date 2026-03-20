using Domain.DTOs;

public interface IEducationService
{
    Task<Response<string>> AddAsync(CreateEducationDto dto);
    Task<Response<Education>> GetByIdAsync(int id);
    Task<Response<List<Education>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateEducationDto dto);
    Task<Response<string>> DeleteAsync(int id);

    Task<Response<List<Education>>> GetByProfileIdAsync(int profileId);
}