using Domain.DTOs;

public interface IJobCategoryService
{
    Task<Response<JobCategoryResponseDto>> CreateAsync(CreateJobCategoryDto dto);
    Task<Response<JobCategoryResponseDto>> GetByIdAsync(int id);
    Task<Response<List<JobCategoryResponseDto>>> GetAllAsync();
    Task<Response<JobCategoryResponseDto>> UpdateAsync(int id, UpdateJobCategoryDto dto);
    Task<Response<bool>> DeleteAsync(int id);
}