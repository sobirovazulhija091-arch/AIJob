namespace Domain.Entities;

public class Recommendation
{
    public int Id { get; set; }
    public int AuthorId { get; set; }    // User who wrote it
    public int RecipientId { get; set; } // User it's about
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
