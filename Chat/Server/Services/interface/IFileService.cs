using Chat.Server.Data.Entity;
using Chat.Shared.Dto;
using Chat.Shared.Request;
namespace Chat.Server.Services.Interface;

public interface IFileService
{
    Task<UploadResult> UploadFile(IFormFile file, string userId, CancellationToken cancellationToken);
    Task<Attachment> CreateAttachment(UploadResult uploadResult, string userId);
    Task<string> GenerateSasUri(string fileKey);
    string GetBlobCDNUrl(string BlobName);

}