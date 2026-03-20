namespace Domain.DTOs;

public class CreateLanguageDto
{
    public string Name { get; set; } = null!;
    public LanguageType Type { get; set; }
}

