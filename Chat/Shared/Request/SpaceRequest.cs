using System.ComponentModel.DataAnnotations;

namespace Chat.Shared.Request;

public class SpaceRequest
{
    [Required]
    [MaxLength(32, ErrorMessage = "Name is too long.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(256, ErrorMessage = "Description is too long.")]
    public string Description { get; set; } = string.Empty;
    public IEnumerable<string> Members { get; set; } = Enumerable.Empty<string>();
}