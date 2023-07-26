namespace Chat.Server.Exceptions;

public class EnvironmentVariableException : Exception
{
    public EnvironmentVariableException(string variableName) : base($"Environment variable {variableName} not found")
    {

    }
}