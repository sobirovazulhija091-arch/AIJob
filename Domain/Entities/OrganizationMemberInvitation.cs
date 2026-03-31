public class OrganizationMemberInvitation
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int InvitedUserId { get; set; }
    public int InvitedByUserId { get; set; }
    public string Role { get; set; } = null!;
    public OrganizationMemberInviteStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
