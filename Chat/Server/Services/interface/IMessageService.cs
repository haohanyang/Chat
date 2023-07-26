using Chat.Shared.Dto;
using Chat.Shared.Request;
namespace Chat.Server.Services.Interface;

public interface IMessageService
{
    Task<(MessageDto, IEnumerable<string> recipientIds)> SaveMessage(MessageRequest request, CancellationToken cancellationToken);
}