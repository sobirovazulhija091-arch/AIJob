using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IOrganizationService
{
    Task<Response<Organization>> CreateAsync(CreateOrganizationDto dto);
    Task<Response<Organization>> GetByIdAsync(int id);
    Task<Response<List<Organization>>> GetAllAsync();
    /// <summary>Organizations the user belongs to (for employer recruiting, not the public directory list).</summary>
    Task<Response<List<Organization>>> GetForUserAsync(int userId);
    Task<PagedResult<Organization>> GetPagedAsync(OrganizationFilter filter, PagedQuery querypage);
    Task<Response<string>> UpdateAsync(int id, UpdateOrganizationDto dto, int actingUserId);
    Task<Response<string>> DeleteAsync(int id, int actingUserId);
    Task<Response<List<Organization>>> SearchByNameAsync(string name);
}