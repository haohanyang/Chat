namespace Chat.Shared.Dto;
public class UploadResult
{
    public string FileName { get; set; } = null!;
    public string ContainerName { get; set; } = null!;
    public string BlobName { get; set; } = null!;
}