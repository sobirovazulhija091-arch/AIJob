namespace Domain.DTOs;

public class UserSettingsDto
{
    public string Theme { get; set; } = "light";
    public string BrandColor { get; set; } = "blue";
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public string Language { get; set; } = "en";
}

public class UpdateUserSettingsDto
{
    public string Theme { get; set; } = "light";
    public string BrandColor { get; set; } = "blue";
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public string Language { get; set; } = "en";
}
