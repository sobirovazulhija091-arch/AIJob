namespace Domain.DTOs;

public class UserPublicProfileDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
