namespace Domain.DTOs;

/// <summary>Safe user shape for API responses — no password or hash fields.</summary>
public class UserResponseDto
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    /// <summary>Primary Identity role for admin listings (Candidate, Organization, Admin).</summary>
    public string? AccountRole { get; set; }
}
