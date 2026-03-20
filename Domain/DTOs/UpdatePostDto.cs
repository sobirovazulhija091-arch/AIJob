namespace Domain.DTOs;

public class UpdatePostDto
{
    public string Content { get; set; } = null!;
    public string? ImageUrl { get; set; }
}
