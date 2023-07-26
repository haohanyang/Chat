using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Chat.Server.Message;
using Chat.Shared.Request;
using Chat.Server.Services.Interface;
using Chat.Shared.Dto;
using Chat.Shared.Message;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Chat.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/message")]
[ServiceFilter(typeof(ExceptionFilter))]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly IMessageService _messageService;
    public MessageController(IHubContext<ChatHub, IChatClient> hubContext,
        IMessageService messageService, ILogger<MessageController> logger)
    {
        _hubContext = hubContext;
        _messageService = messageService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> SaveMessage([FromBody] MessageRequest request, CancellationToken cancellationToken)
    {
        var principalId = User.Claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier)?.Value;
        if (principalId != request.AuthorId)
            throw new ArgumentException("User " + request.AuthorId + " is not the current user");

        var (message, recipients) = await _messageService.SaveMessage(request, cancellationToken);
        if (request.Type == ChatType.Direct)
        {
            await _hubContext.Clients.User(recipients.First()).ReceiveMessage(message);
            _logger.LogInformation("User {} sent a message to direct chat {}", request.AuthorId, request.ChatId);
        }
        else
        {
            await Task.WhenAll(
                recipients.Select(async id =>
                    await _hubContext.Clients.User(id).ReceiveMessage(message)));
            _logger.LogInformation("User {} sent a message to space {}", request.AuthorId, request.ChatId);
        }
        return StatusCode(201, message);
    }
}