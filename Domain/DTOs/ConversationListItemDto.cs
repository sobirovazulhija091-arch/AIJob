namespace Domain.DTOs;

/// <summary>Conversation row for inbox list with unread and last-message preview.</summary>
public class ConversationListItemDto
{
    public int Id { get; set; }
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>Messages from the other participant not yet read by the current user.</summary>
    public int UnreadCount { get; set; }
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAt { get; set; }
}
