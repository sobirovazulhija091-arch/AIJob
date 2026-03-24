using Domain.Entities;

namespace Domain.DTOs;

public class JobWithMatchDto
{
    public Job Job { get; set; } = null!;
    public int MatchScore { get; set; }  
    public string? MatchSummary { get; set; }  
}
