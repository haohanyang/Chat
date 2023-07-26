namespace Chat.Shared.Dto;

public class SpaceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; }
    public IEnumerable<UserDto> Members { get; set; } = Enumerable.Empty<UserDto>();
}