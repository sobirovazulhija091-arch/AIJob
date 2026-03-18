namespace Domain.DTOs;

public class CreateOrganizationMemberDto
{
    public int OrganizationId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = null!;
}

