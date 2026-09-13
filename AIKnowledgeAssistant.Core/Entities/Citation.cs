namespace AIKnowledgeAssistant.Core.Entities;

public class Citation
{
    public Guid Id { get; set; }
    public Guid ChatMessageId { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public double RelevanceScore { get; set; }
}
