namespace Domain.DTOs;

public class CreateRefreshTokenDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

