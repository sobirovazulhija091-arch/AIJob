namespace Domain.DTOs;

public class CreateProfileLanguageDto
{
    public int ProfileId { get; set; }
    public int LanguageId { get; set; }
    public Languagelevel Level { get; set; } 
}

