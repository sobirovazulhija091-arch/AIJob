namespace Domain.DTOs;

public class OrganizationMemberResponseDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = null!;
}

