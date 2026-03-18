namespace Domain.DTOs;

public class JobApplicationResponseDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int UserId { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
}

