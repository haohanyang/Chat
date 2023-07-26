namespace Chat.Server.Data.Entity;

public class SpaceMembership
{
    public int Id { get; set; }
    public string MemberId { get; set; } = null!;
    public User Member { get; set; } = null!;
    public int SpaceId { get; set; }
    public Space Space { get; set; } = null!;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public bool IsOwner { get; set; }
}