namespace Chat.Server.Data.Entity;

public class Space
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;

    public ICollection<SpaceMembership> Memberships { get; set; } = new HashSet<SpaceMembership>();

    public ICollection<SpaceMessage> Messages { get; set; } = new HashSet<SpaceMessage>();
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;
}