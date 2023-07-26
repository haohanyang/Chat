namespace Chat.Server.Data.Entity;

public class Attachment
{
    public int Id { get; set; }
    public User Uploader { get; set; } = null!;
    public string UploaderId { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContainerName { get; set; } = null!;
    public string BlobName { get; set; } = null!;
    public long Size { get; set; }
    public string ContentType { get; set; } = null!;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

}

public class DirectMessageAttachment : Attachment
{
    public int DirectMessageId { get; set; }
    public DirectMessage DirectMessage { get; set; } = null!;

}

public class SpaceMessageAttachment : Attachment
{
    public int SpaceMessageId { get; set; }
    public SpaceMessage SpaceMessage { get; set; } = null!;

}
