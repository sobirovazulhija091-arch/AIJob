using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IJobService
{
    Task<Response<JobResponseDto>> CreateAsync(CreateJobDto dto);
    Task<Response<JobResponseDto>> GetByIdAsync(int id);
    Task<Response<IEnumerable<JobResponseDto>>> GetAllAsync();
    Task<Response<PagedResult<JobResponseDto>>> GetPagedAsync(JobFilter filter);
    Task<Response<JobResponseDto>> UpdateAsync(int id, UpdateJobDto dto);
    Task<Response<JobResponseDto>> DeleteAsync(int id);

    Task<Response<List<JobResponseDto>>> GetByOrganizationIdAsync(int organizationId);
    Task<Response<List<JobResponseDto>>> SearchByTitleAsync(string title);
}