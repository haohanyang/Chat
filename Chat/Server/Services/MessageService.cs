using Chat.Server.Data;
using Chat.Server.Data.Entity;
using Chat.Server.Services.Interface;
using Chat.Shared;
using Chat.Shared.Dto;
using Chat.Shared.Request;

namespace Chat.Server.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileService _fileService;

    public MessageService(ApplicationDbContext dbContext, IFileService fileService)
    {
        _dbContext = dbContext;
        _fileService = fileService;
    }

    public async Task<(MessageDto, IEnumerable<string>)> SaveMessage(MessageRequest request, CancellationToken cancellationToken)
    {
        var author = await _dbContext.Users.FindAsync(request.AuthorId, cancellationToken);
        if (author == null)
            throw new ArgumentException("User " + request.AuthorId + " not found");

        var attachmentUploadResult = request.AttachmentUploadResult;

        if (request.Type == ChatType.Direct)
        {
            var directChat = await _dbContext.DirectChats.FindAsync(request.ChatId, cancellationToken);
            if (directChat == null)
                throw new ArgumentException("Direct chat " + request.ChatId + " not found");

            if (directChat.User1Id != author.Id && directChat.User2Id != author.Id)
                throw new ArgumentException("User " + request.AuthorId + " is not a member of chat " + request.ChatId);

            DirectMessageAttachment? attachment = null;
            if (attachmentUploadResult != null)
            {
                var result = await _fileService.CreateAttachment(attachmentUploadResult, author.Id);
                attachment = new DirectMessageAttachment
                {
                    ContentType = result.ContentType,
                    Size = result.Size,
                    TimeStamp = result.TimeStamp,
                    Uploader = author,
                    FileName = result.FileName,
                    ContainerName = result.ContainerName,
                    BlobName = result.BlobName,
                };
            }

            var message = new DirectMessage
            {
                Author = author,
                DirectChat = directChat,
                Content = request.Content,
                Attachment = attachment,
            };

            await _dbContext.DirectMessages.AddAsync(message, cancellationToken);
            directChat.Updated = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return (new MessageDto
            {
                Id = message.Id,
                Author = author.ToDto(),
                ChatId = directChat.Id,
                Content = message.Content,
                TimeStamp = message.TimeStamp.ToString(Constants.DateFormat),
                Type = ChatType.Direct,
                AttachmentUri = attachment != null ? _fileService.GetBlobCDNUrl(attachment.BlobName) : null,
            }, new List<string> { author.Id == directChat.User1Id ? directChat.User2Id : directChat.User1Id });
        }
        else
        {
            var space = await _dbContext.Spaces.FindAsync(request.ChatId, cancellationToken);
            if (space == null)
                throw new ArgumentException("Space " + request.ChatId + " not found");

            var memberIds = _dbContext.SpaceMemberships.Where(e => e.Space == space)
                .Select(e => e.MemberId).ToHashSet();

            if (memberIds == null || !memberIds.Contains(author.Id))
                throw new ArgumentException("User " + author.Id + " is not a member of space " + request.ChatId);

            SpaceMessageAttachment? attachment = null;

            if (attachmentUploadResult != null)
            {
                var result = await _fileService.CreateAttachment(attachmentUploadResult, author.Id);
                attachment = new SpaceMessageAttachment
                {
                    ContentType = result.ContentType,
                    Size = result.Size,
                    TimeStamp = result.TimeStamp,
                    Uploader = author,
                    FileName = result.FileName,
                    ContainerName = result.ContainerName,
                    BlobName = result.BlobName,
                };
            }

            var message = new SpaceMessage
            {
                Author = author,
                Space = space,
                Content = request.Content,
                Attachment = attachment,
            };

            await _dbContext.SpaceMessages.AddAsync(message, cancellationToken);
            space.Updated = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return (new MessageDto
            {
                Id = message.Id,
                Author = author.ToDto(),
                ChatId = space.Id,
                Content = message.Content,
                TimeStamp = message.TimeStamp.ToString(Constants.DateFormat),
                Type = ChatType.Space,
                Name = space.Name,
                AttachmentUri = attachment != null ? _fileService.GetBlobCDNUrl(attachment.BlobName) : null,
            }, memberIds.Where(e => e != author.Id));
        }
    }
}
