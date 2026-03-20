namespace Domain.DTOs;

public class CreateJobDto
{
    public int OrganizationId { get; set; }
    public string Title { get; set; }=null!;
    public string? Description { get; set; }
    public decimal SalaryMin { get; set; }
    public decimal SalaryMax { get; set; }
    public string? Location { get; set; }
    public JobType JobType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public int ExperienceRequired { get; set; }
    public int CategoryId { get; set; }
}

 


