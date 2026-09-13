using AIKnowledgeAssistant.Core.Entities;

namespace AIKnowledgeAssistant.Core.Interfaces;

public interface ISearchIndexService
{
    Task UploadChunksAsync(Guid documentId, string fileName, IReadOnlyList<DocumentChunk> chunks, IReadOnlyList<float[]> embeddings);
    Task<IReadOnlyList<DocumentChunk>> SearchAsync(string query, float[] queryVector, int top = 5);
}
