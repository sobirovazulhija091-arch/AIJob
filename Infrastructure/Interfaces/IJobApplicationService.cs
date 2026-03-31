using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IJobApplicationService
{
    Task<Response<string>> CreateAsync(CreateJobApplicationDto dto);
    Task<Response<JobApplication>> GetByIdAsync(int id);
    Task<PagedResult<JobApplication>> GetPagedAsync(JobApplicationFilter filter, PagedQuery querypage);
    Task<Response<string>> UpdateAsync(int id, UpdateJobApplicationDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<JobApplication>>> GetByUserIdAsync(int userId);
    Task<Response<List<JobApplication>>> GetByJobIdAsync(int jobId, int actingUserId);
    Task<Response<string>> ChangeStatusAsync(int id, ApplicationStatus status, int actingUserId);
}