using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IJobApplicationService
{
    Task<Response<string>> CreateAsync(CreateJobApplicationDto dto);
    Task<Response<JobApplication>> GetByIdAsync(int id);
    Task<Response<List<JobApplication>>> GetAllAsync();
    Task<PagedResult<JobApplication>> GetPagedAsync(JobApplicationFilter filter, PagedQuery querypage);
    Task<Response<string>> UpdateAsync(int id, UpdateJobApplicationDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<JobApplication>>> GetByUserIdAsync(int userId);
    Task<Response<List<JobApplication>>> GetByJobIdAsync(int jobId);
    Task<Response<string>> ChangeStatusAsync(int id, ApplicationStatus status);
}