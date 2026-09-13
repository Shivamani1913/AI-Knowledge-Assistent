using AIKnowledgeAssistant.Core.Enums;

namespace AIKnowledgeAssistant.Core.Entities;

public class OrgDocument
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string BlobUri { get; set; } = string.Empty;
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
    public ProcessingStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public int ChunkCount { get; set; }
    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}
