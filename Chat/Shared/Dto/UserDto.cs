namespace Chat.Shared.Dto;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;

    public string Name => $"{FirstName} {LastName}";

    public override bool Equals(object? obj)
    {
        if (obj is UserDto other)
        {
            return Id == other.Id;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}