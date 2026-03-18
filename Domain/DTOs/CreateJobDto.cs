namespace Domain.DTOs;

public class CreateJobDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal SalaryMin { get; set; }
    public decimal SalaryMax { get; set; }
    public int ExperienceRequired { get; set; }
    public int CategoryId { get; set; }
    public int OrganizationId { get; set; }
}

 


