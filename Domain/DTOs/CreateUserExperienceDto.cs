namespace Domain.DTOs;

public class CreateUserExperienceDto
{
    public int UserId { get; set; }
    public string CompanyName { get; set; } = null!;
    public string Position { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

