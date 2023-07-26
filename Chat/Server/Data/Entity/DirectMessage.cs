namespace Chat.Server.Data.Entity;

public class DirectMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = null!;
    public User Author { get; set; } = null!;
    public int DirectChatId { get; set; }
    public DirectChat DirectChat { get; set; } = null!;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public DirectMessageAttachment? Attachment { get; set; }

}