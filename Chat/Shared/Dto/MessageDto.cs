namespace Chat.Shared.Dto;

public class MessageDto
{
    public int Id { get; set; }
    public UserDto Author { get; set; } = null!;
    public ChatType Type { get; set; }
    public int ChatId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string TimeStamp { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AttachmentUri { get; set; }
    public string Key => $"{Type}-{ChatId}-{Id}";
}