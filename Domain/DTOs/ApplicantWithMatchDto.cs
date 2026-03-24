using Domain.Entities;

namespace Domain.DTOs;

public class ApplicantWithMatchDto
{
    public JobApplication Application { get; set; } = null!;
    public User User { get; set; } = null!;
    public string? UserProfileAbout { get; set; }
    public int ExperienceYears { get; set; }
    public int MatchScore { get; set; }  
    public string? MatchSummary { get; set; }
}
