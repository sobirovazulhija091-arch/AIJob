namespace Domain.DTOs;

public class CreateRecommendationDto
{
    public int RecipientId { get; set; }
    public string Content { get; set; } = null!;
}
