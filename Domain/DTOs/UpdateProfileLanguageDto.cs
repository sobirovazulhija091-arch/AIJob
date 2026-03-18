namespace Domain.DTOs;

public class UpdateProfileLanguageDto
{
    public int Id { get; set; }
    public int LanguageId { get; set; }
    public string Level { get; set; } = null!;
}

