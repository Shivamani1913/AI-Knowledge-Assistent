using System.Threading.Channels;
using System.Text.Json;
using AIKnowledgeAssistant.Core.Entities;
using AIKnowledgeAssistant.Core.Enums;
using AIKnowledgeAssistant.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AIKnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController(
    AppDbContext db,
    Channel<Guid> queue,
    IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest("Empty file");
        }

        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var savedFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var savedPath = Path.Combine(uploadsDir, savedFileName);

        using (var stream = new FileStream(savedPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        var doc = new OrgDocument
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            BlobUri = savedPath,
            UploadedByUserId = userId,
            UploadedAt = DateTimeOffset.UtcNow,
            Status = ProcessingStatus.Queued
        };

        db.Documents.Add(doc);
        await db.SaveChangesAsync();

        await queue.Writer.WriteAsync(doc.Id);

        return Accepted(new { documentId = doc.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var doc = await db.Documents.FindAsync(id);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpGet("{id}/chunks")]
    public async Task<IActionResult> GetChunks(Guid id)
    {
        var rawChunks = await db.Chunks
            .Where(c => c.DocumentId == id)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync();

        var chunks = rawChunks.Select(c => new
        {
            c.ChunkIndex,
            ContentPreview = c.Content.Length > 100 ? c.Content.Substring(0, 100) + "..." : c.Content,
            EmbeddingLength = c.EmbeddingJson.Length > 0
                ? JsonSerializer.Deserialize<float[]>(c.EmbeddingJson)!.Length
                : 0
        }).ToList();

        return Ok(chunks);
    }
}

