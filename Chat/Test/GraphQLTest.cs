namespace Chat.Test;

public class GraphQLTest
{
    private readonly ITestOutputHelper _output;

    public GraphQLTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task FetchAuthor()
    {
        var result = await TestServices.ExecuteRequestAsync(
            b => b.SetQuery("{ users { id } }"));
        _output.WriteLine(result);
    }
}
