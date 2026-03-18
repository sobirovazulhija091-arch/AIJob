using Domain.DTOs;

namespace Domain.Filters;

public class OrganizationFilter : PagedRequest
{
    public string? Name { get; set; }
}

