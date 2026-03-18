namespace Domain.DTOs;

public class UpdateUserEducationDto
{
    public int Id { get; set; }
    public string Institution { get; set; } = null!;
    public string Degree { get; set; } = null!;
    public int StartYear { get; set; }
    public int EndYear { get; set; }
}

