using Microsoft.AspNetCore.Mvc;
using SmartDocs.Application.Services;

namespace SmartDocs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;

    public DocumentsController(DocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file.");

        using var stream = file.OpenReadStream();

        var documentId = await _documentService.UploadDocumentAsync(
            stream,
            file.FileName,
            file.ContentType,
            HttpContext.RequestAborted);

        return Accepted(new { DocumentId = documentId });
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var document = await _documentService.GetDocumentAsync(id);

        if (document == null)
            return NotFound();

        return Ok(document);
    }

    [HttpGet("sanityCheck")]
    public IActionResult Health()
    {
        return Ok("API is running");
    }

}
