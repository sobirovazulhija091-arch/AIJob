using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IJobService
{
    Task<Response<string>> AddAsync(CreateJobDto dto, int actingUserId);
    Task<Response<Job>> GetByIdAsync(int id);
    Task<Response<List<Job>>> GetAllAsync();
    Task<Response<List<Job>>> GetForUserAsync(int actingUserId);
    Task<PagedResult<Job>> GetPagedAsync(JobFilter filter,PagedQuery querypage);
    Task<Response<string>> UpdateAsync(int id, UpdateJobDto dto, int actingUserId);
    Task<Response<string>> DeleteAsync(int id, int actingUserId);
    Task<Response<List<Job>>> GetByOrganizationIdAsync(int organizationId);
    Task<Response<List<Job>>> SearchByTitleAsync(string title);
}