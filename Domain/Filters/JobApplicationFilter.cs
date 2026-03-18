using Domain.DTOs;

namespace Domain.Filters;

public class JobApplicationFilter : PagedRequest
{
    public ApplicationStatus? Status { get; set; }
    public int? JobId { get; set; }
    public int? UserId { get; set; }
}

