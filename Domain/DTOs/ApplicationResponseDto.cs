namespace Domain.DTOs;
public class ApplicationResponseDto
{
    public int Id { get; set; }
    public string Status { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
}