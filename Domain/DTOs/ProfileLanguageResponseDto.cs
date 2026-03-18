namespace Domain.DTOs;

public class ProfileLanguageResponseDto
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public int LanguageId { get; set; }
    public string Level { get; set; } = null!;
}

