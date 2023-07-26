using System.ComponentModel.DataAnnotations;
using Chat.Shared.Dto;
namespace Chat.Shared.Request;

public class MessageRequest
{
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    [Required]
    [MaxLength(512, ErrorMessage = "Content is too long.")]
    public string Content { get; set; } = string.Empty;
    [Required]
    public ChatType Type { get; set; }
    [Required]
    public int ChatId { get; set; }
    public UploadResult? AttachmentUploadResult { get; set; }
}
