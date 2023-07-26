using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Chat.Client.Security;

public class MockAccessTokenProvider : IAccessTokenProvider
{
    public ValueTask<AccessTokenResult> RequestAccessToken()
    {
        var status = AccessTokenResultStatus.Success;
        var token = new AccessToken { Value = "mock" };
        var result = new AccessTokenResult(status, token, "", new InteractiveRequestOptions { ReturnUrl = "", Interaction = InteractionType.GetToken });
        return new ValueTask<AccessTokenResult>(result);
    }
    public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
    {
        return RequestAccessToken();
    }
}