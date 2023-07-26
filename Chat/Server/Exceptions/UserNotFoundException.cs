namespace Chat.Server.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string userId) : base($"User {userId} not found")
    {

    }
}