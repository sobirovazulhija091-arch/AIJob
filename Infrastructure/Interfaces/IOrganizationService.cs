using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IOrganizationService
{
    Task<Response<string>> CreateAsync(CreateOrganizationDto dto);
    Task<Response<Organization>> GetByIdAsync(int id);
    Task<Response<List<Organization>>> GetAllAsync();
    Task<PagedResult<Organization>> GetPagedAsync(OrganizationFilter filter, PagedQuery querypage);
    Task<Response<string>> UpdateAsync(int id, UpdateOrganizationDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<Organization>>> SearchByNameAsync(string name);
}