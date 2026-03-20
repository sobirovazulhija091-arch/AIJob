namespace Domain.Filters;

public class JobApplicationFilter
{
    public ApplicationStatus? Status { get; set; }
    public int? JobId { get; set; }
    public int? UserId { get; set; }
}

