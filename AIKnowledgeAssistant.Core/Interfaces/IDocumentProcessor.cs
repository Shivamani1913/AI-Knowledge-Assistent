namespace AIKnowledgeAssistant.Core.Interfaces;

public interface IDocumentProcessor
{
    Task<string> ExtractAsync(string blobUri);
}
