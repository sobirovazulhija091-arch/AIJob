using Domain.DTOs;

public interface IOrganizationMemberService
{
    Task<Response<OrganizationMemberResponseDto>> CreateAsync(CreateOrganizationMemberDto dto);
    Task<Response<OrganizationMemberResponseDto>> GetByIdAsync(int id);
    Task<Response<List<OrganizationMemberResponseDto>>> GetAllAsync();
    Task<Response<OrganizationMemberResponseDto>> UpdateAsync(int id, UpdateOrganizationMemberDto dto);
    Task<Response<OrganizationMemberResponseDto>> DeleteAsync(int id);
    Task<Response<List<OrganizationMemberResponseDto>>> GetByOrganizationIdAsync(int organizationId);
}