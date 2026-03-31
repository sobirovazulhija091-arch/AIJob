public class Conversation
{
    public int Id { get; set; }
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>Latest message id user1 has opened in this thread (inclusive).</summary>
    public int? User1LastReadMessageId { get; set; }
    /// <summary>Latest message id user2 has opened in this thread (inclusive).</summary>
    public int? User2LastReadMessageId { get; set; }
}
