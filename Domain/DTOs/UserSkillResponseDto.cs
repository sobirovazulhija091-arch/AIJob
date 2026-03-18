namespace Domain.DTOs;

public class UserSkillResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = null!;
}

