namespace Domain.Entities;

public class Endorsement
{
    public int Id { get; set; }
    public int EndorserId { get; set; }      // User who endorsed
    public int ProfileSkillId { get; set; }  // Skill on profile being endorsed
    public DateTime CreatedAt { get; set; }
}
