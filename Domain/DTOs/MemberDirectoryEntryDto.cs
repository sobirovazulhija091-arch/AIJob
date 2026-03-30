namespace Domain.DTOs;

/// <summary>Minimal member info for any authenticated user (directory / discovery).</summary>
public class MemberDirectoryEntryDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    /// <summary>Primary Identity role: Candidate, Organization, or Admin.</summary>
    public string Role { get; set; } = null!;
}
