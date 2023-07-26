namespace Chat.Shared.Dto;

public class DirectChatDto
{
    public int Id { get; set; }
    public string User1Id { get; set; } = null!;
    public string User2Id { get; set; } = null!;
    public UserDto? User1 { get; set; }
    public UserDto? User2 { get; set; }

    public DateTime TimeStamp { get; set; }

    public override bool Equals(object? obj)
    {

        if (obj is DirectChatDto other)
        {
            return Id == other.Id;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public DirectChatDto()
    {
    }
    public DirectChatDto(UserDto user1, UserDto user2)
    {
        if (user1.Id.CompareTo(user2.Id) < 0)
        {
            User1 = user1;
            User2 = user2;
        }
        else
        {
            User1 = user2;
            User2 = user1;
        }
    }
}