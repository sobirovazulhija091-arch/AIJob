namespace Domain.DTOs;

public class CreateOrganizationDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public string? LogoUrl { get; set; }
}

