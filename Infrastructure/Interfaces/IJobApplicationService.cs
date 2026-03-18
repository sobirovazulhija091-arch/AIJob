using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IJobApplicationService
{
    Task<Response<JobApplicationDto>> CreateAsync(CreateJobApplicationDto dto);
    Task<Response<JobApplicationDto>> GetByIdAsync(int id);
    Task<Response<List<JobApplicationDto>>> GetAllAsync();
    Task<Response<PagedResult<JobApplicationDto>>> GetPagedAsync(JobApplicationFilter filter);
    Task<Response<JobApplicationDto>> UpdateAsync(int id, UpdateJobApplicationDto dto);
    Task<Response<bool>> DeleteAsync(int id);

    Task<Response<List<JobApplicationResponseDto>>> GetByUserIdAsync(int userId);
    Task<Response<List<JobApplicationResponseDto>>> GetByJobIdAsync(int jobId);
    Task<Response<bool>> ChangeStatusAsync(int id, ApplicationStatus status);
}