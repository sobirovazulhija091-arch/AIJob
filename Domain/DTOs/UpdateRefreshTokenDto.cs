namespace Domain.DTOs;

public class UpdateRefreshTokenDto
{
    public int Id { get; set; }
    public DateTime ExpiresAt { get; set; }
}

