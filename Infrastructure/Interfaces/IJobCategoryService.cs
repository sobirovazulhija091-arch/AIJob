using Domain.DTOs;

public interface IJobCategoryService
{
    Task<Response<string>> CreateAsync(CreateJobCategoryDto dto);
    Task<Response<JobCategory>> GetByIdAsync(int id);
    Task<Response<List<JobCategory>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateJobCategoryDto dto);
    Task<Response<string>> DeleteAsync(int id);
}