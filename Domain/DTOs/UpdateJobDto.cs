namespace Domain.DTOs;

public class UpdateJobDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal SalaryMin { get; set; }
    public decimal SalaryMax { get; set; }
}
