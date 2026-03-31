using Domain.DTOs;

public interface IOrganizationMemberService
{
    Task<Response<string>> CreateAsync(CreateOrganizationMemberDto dto, int actingUserId);
    Task<Response<string>> InviteAsync(CreateOrganizationMemberDto dto, int actingUserId);
    Task<Response<string>> RespondToInvitationAsync(int invitationId, int userId, OrganizationMemberInviteRespondDto dto);
    Task<Response<OrganizationMember>> GetByIdAsync(int id);
    Task<Response<List<OrganizationMember>>> GetAllAsync();
    Task<Response<string>> UpdateAsync(int id, UpdateOrganizationMemberDto dto);
    Task<Response<string>> DeleteAsync(int id);
    Task<Response<List<OrganizationMember>>> GetByOrganizationIdAsync(int organizationId);
}
