namespace Domain.DTOs;

public class CreateEducationDto
{
    public int ProfileId { get; set; }
    public string SchoolName { get; set; } = null!;
    public string Degree { get; set; } = null!;
    public string FieldOfStudy { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Grade { get; set; } = null!;
}

