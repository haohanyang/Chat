namespace Chat.Server.Data.Entity;

public class DirectChat
{
    public int Id { get; set; }

    public string User1Id { get; set; } = null!;
    public User User1 { get; set; } = null!;

    public string User2Id { get; set; } = null!;
    public User User2 { get; set; } = null!;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;

    public ICollection<DirectMessage> Messages { get; set; } = new HashSet<DirectMessage>();

    public DirectChat()
    {
    }
}