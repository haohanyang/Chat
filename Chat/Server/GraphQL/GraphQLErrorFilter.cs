namespace Chat.Server.GraphQL;

public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        return error.WithMessage(error.Exception?.Message ?? "Unknown error");
    }
}