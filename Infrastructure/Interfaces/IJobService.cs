using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IJobService
{
    Task<Response<string>> AddAsync(CreateJobDto dto);
    Task<Response<Job>> GetByIdAsync(int id);
    Task<Response<List<Job>>> GetAllAsync();
    Task<PagedResult<Job>> GetPagedAsync(JobFilter filter,PagedQuery querypage);
    Task<Response<string>> UpdateAsync(int id, UpdateJobDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<Job>>> GetByOrganizationIdAsync(int organizationId);
    Task<Response<List<Job>>> SearchByTitleAsync(string title);
}