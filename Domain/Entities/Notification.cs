public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; }=null!;
    public string Message { get; set; }=null!;
    /// <summary>Optional link to related row (e.g. Connection.Id for connection-request notifications).</summary>
    public int? RelatedId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
