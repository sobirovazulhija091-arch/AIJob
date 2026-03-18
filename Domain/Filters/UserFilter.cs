using Domain.DTOs;

namespace Domain.Filters;

public class UserFilter
    : PagedRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
}