namespace AIKnowledgeAssistant.Core.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public bool IsUser { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }
    public ICollection<Citation> Citations { get; set; } = new List<Citation>();
    public int? LatencyMs { get; set; }
}
