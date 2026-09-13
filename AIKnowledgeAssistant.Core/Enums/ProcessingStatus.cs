namespace AIKnowledgeAssistant.Core.Enums;

public enum ProcessingStatus
{
    Queued,
    Extracting,
    Chunking,
    Embedding,
    Indexing,
    Completed,
    Failed
}
