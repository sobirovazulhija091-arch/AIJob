namespace Domain.DTOs;

public class CreateProfileDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Headline { get; set; } = null!;
    public string About { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public string? BackgroundPhotoUrl { get; set; }
    public DateTime BirthDate { get; set; }
}

