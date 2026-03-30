namespace Domain.DTOs;

/// <summary>Profile shown when viewing another member (LinkedIn-style); excludes salary and CV.</summary>
public class MemberProfileDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string AboutMe { get; set; } = "";
    public int ExperienceYears { get; set; }
}
