namespace Domain.DTOs;

public class CreateMessageDto
{
    public int ConversationId { get; set; }
    public string Content { get; set; } = null!;
}

