using Domain.DTOs;

namespace Domain.Filters;

public class JobFilter 
{
    public string? Title { get; set; }
    public string? Location { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public JobType? JobType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public int? OrganizationId { get; set; }
    public int? CategoryId { get; set; }
}

