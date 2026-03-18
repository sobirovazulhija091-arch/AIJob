using Domain.DTOs;
using Domain.Filters;
using Infrastructure.Responses;

public interface IOrganizationService
{
    Task<Response<OrganizationResponseDto>> CreateAsync(CreateOrganizationDto dto);
    Task<Response<OrganizationResponseDto>> GetByIdAsync(int id);
    Task<Response<IEnumerable<OrganizationResponseDto>>> GetAllAsync();
    Task<Response<PagedResult<OrganizationResponseDto>>> GetPagedAsync(OrganizationFilter filter);
    Task<Response<OrganizationResponseDto>> UpdateAsync(int id, UpdateOrganizationDto dto);
    Task<Response<OrganizationMemberResponseDto>> DeleteAsync(int id);

    Task<Response<List<OrganizationResponseDto>>> SearchByNameAsync(string name);
}