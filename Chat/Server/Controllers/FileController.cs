using System.Net;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Chat.Shared.Dto;
using Path = System.IO.Path;
using Chat.Server.Services.Interface;
using System.Security.Claims;

namespace Chat.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/file")]
[ServiceFilter(typeof(ExceptionFilter))]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    public async Task<ActionResult<UploadResult>> UploadFile([FromForm] IEnumerable<IFormFile> files, CancellationToken cancellationToken)
    {
        var principalId = User.Claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier)?.Value;
        if (principalId == null)
            throw new ArgumentException("User is not authenticated");
        return await _fileService.UploadFile(files.First(), principalId, cancellationToken);
    }
}