using Events.DATA.DTOs.File;
using Events.Services;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class FileController : Properties.BaseController
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] FileForm fileForm) => Ok(await _fileService.Upload(fileForm));

    [HttpPost("multi")]
    public async Task<IActionResult> Upload([FromForm] MultiFileForm filesForm) =>
        Ok(await _fileService.Upload(filesForm));
}