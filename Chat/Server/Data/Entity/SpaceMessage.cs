namespace Chat.Server.Data.Entity;

public class SpaceMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public string AuthorId { get; set; } = null!;
    public User Author { get; set; } = null!;
    public int SpaceId { get; set; }
    public Space Space { get; set; } = null!;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public SpaceMessageAttachment? Attachment { get; set; }
}