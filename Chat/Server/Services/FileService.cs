using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Chat.Server.Data.Entity;
using Chat.Server.Services.Interface;
using Chat.Shared.Dto;

namespace Chat.Server.Services;

public class FileService : IFileService
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<FileService> _logger;
    private string? _azureCDNEndpointName;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
        var storageConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        var blobContainerName = Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER_NAME");
        _azureCDNEndpointName = Environment.GetEnvironmentVariable("AZURE_CDN_ENDPOINT_NAME");

        if (string.IsNullOrEmpty(storageConnectionString))
        {
            throw new Exception("AZURE_STORAGE_CONNECTION_STRING is not set");
        }

        if (string.IsNullOrEmpty(blobContainerName))
        {
            throw new Exception("AZURE_BLOB_CONTAINER_NAME is not set");
        }

        if (string.IsNullOrEmpty(_azureCDNEndpointName))
        {
            throw new Exception("AZURE_CDN_ENDPOINT_NAME is not set");
        }


        _blobContainerClient = new BlobContainerClient(storageConnectionString, blobContainerName);

    }

    public async Task<UploadResult> UploadFile(IFormFile file, string userId, CancellationToken cancellationToken)
    {
        var uploadResult = new UploadResult();
        var untrustedFileName = file.FileName;
        uploadResult.FileName = untrustedFileName;
        var trustedFileNameForDisplay =
            System.Net.WebUtility.HtmlEncode(untrustedFileName);

        var blobName = System.Guid.NewGuid().ToString();
        var client = _blobContainerClient.GetBlobClient(blobName);
        var blobInfo = await client.UploadAsync(file.OpenReadStream(),
            new BlobHttpHeaders() { ContentType = file.ContentType },
            new Dictionary<string, string>() { { "uploader", userId } },
            cancellationToken: cancellationToken);
        _logger.LogInformation("{} was uploaded to Azure Blob Storage",
            trustedFileNameForDisplay);
        uploadResult.BlobName = blobName;
        uploadResult.ContainerName = _blobContainerClient.Name;
        return uploadResult;
    }

    public async Task<Attachment> CreateAttachment(UploadResult uploadResult, string userId)
    {
        if (uploadResult.ContainerName != _blobContainerClient.Name)
        {
            throw new ArgumentException("Invalid container name");
        }

        var blob = _blobContainerClient.GetBlobClient(uploadResult.BlobName);
        var exist = await blob.ExistsAsync();
        if (!exist)
        {
            throw new ArgumentException("File does not exist");
        }
        var properties = await blob.GetPropertiesAsync();

        properties.Value.Metadata.TryGetValue("uploader", out var uploader);
        if (uploader != userId)
        {
            throw new ArgumentException("User is not the uploader");
        }

        var attachment = new Attachment();
        attachment.FileName = uploadResult.FileName;
        attachment.ContentType = properties.Value.ContentType;
        attachment.Size = properties.Value.ContentLength;
        attachment.TimeStamp = properties.Value.LastModified.UtcDateTime;
        attachment.ContainerName = _blobContainerClient.Name;
        attachment.BlobName = uploadResult.BlobName;
        return attachment;
    }

    public async Task<string> GenerateSasUri(string fileKey)
    {
        var blobClient = _blobContainerClient.GetBlobClient(fileKey);
        if (!await blobClient.ExistsAsync())
        {
            throw new ArgumentException("File does not exist");
        }

        if (blobClient.CanGenerateSasUri)
        {
            // Create a SAS token that's valid for one day
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _blobContainerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b"
            };

            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(1);
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
        throw new Exception("Cannot generate SAS token");
    }

    public string GetBlobCDNUrl(string BlobName)
    {
        return $"https://{_azureCDNEndpointName}.azureedge.net/{_blobContainerClient.Name}/{BlobName}";
    }
}