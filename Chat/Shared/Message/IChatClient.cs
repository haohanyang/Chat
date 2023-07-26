using Chat.Shared.Dto;

namespace Chat.Shared.Message;

public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);
}
