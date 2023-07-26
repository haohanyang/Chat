using Chat.Shared.Dto;
namespace Chat.Client.Models;

public class Contact
{
    public int Id { get; set; }
    public string Image { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ChatType Type { get; set; }
    public DateTime Time { get; set; }
    public List<MessageDto>? Messages { get; set; } = null;
    public int UnreadMessageCount { get; set; }

    public string Key => $"{Type}-{Id}";
}