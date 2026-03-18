namespace Domain.DTOs;

public class CreateUserSkillDto
{
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = null!;
}

