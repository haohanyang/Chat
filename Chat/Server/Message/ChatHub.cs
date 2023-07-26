using Microsoft.AspNetCore.SignalR;
using Chat.Shared.Message;
using Microsoft.AspNetCore.Authorization;

namespace Chat.Server.Message;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Client connected from " + Context.UserIdentifier);
        await base.OnConnectedAsync();
    }
}