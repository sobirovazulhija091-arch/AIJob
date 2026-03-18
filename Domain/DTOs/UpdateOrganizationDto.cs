namespace Domain.DTOs;

public class UpdateOrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
}

