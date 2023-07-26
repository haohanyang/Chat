namespace Chat.Server.Exceptions;

public class BadCredentialException : Exception
{
    public BadCredentialException() : base("Username or password is incorrect")
    {

    }
}