using System.Text;
using System.Text.Json;
using AIKnowledgeAssistant.Core.Entities;
using AIKnowledgeAssistant.Core.Interfaces;
using AIKnowledgeAssistant.Infrastructure.Data;
using AIKnowledgeAssistant.Infrastructure.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace AIKnowledgeAssistant.Infrastructure.AI;

public class RagService(
    AppDbContext db,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IChatClient chatClient) : IRagService
{
    private const string SystemPrompt = """
        You are an internal knowledge assistant. Answer ONLY using the provided context.
        If the answer isn't in the context, say you don't have that information.
        Never make something up. Cite sources using [1], [2] matching the numbered context blocks.
        """;

    public async IAsyncEnumerable<object> StreamAnswerAsync(Guid sessionId, string userId, string question)
    {
        var questionEmbeddingResult = await embeddingGenerator.GenerateAsync(new[] { question });
        var questionVector = questionEmbeddingResult.First().Vector.ToArray();

        var allChunks = await db.Chunks
            .Where(c => c.EmbeddingJson != "")
            .ToListAsync();

        var scored = allChunks
            .Select(c => new
            {
                Chunk = c,
                Score = VectorMath.CosineSimilarity(questionVector, JsonSerializer.Deserialize<float[]>(c.EmbeddingJson)!)
            })
            .OrderByDescending(x => x.Score)
            .Take(5)
            .ToList();

        var contextBlock = new StringBuilder();
        for (int i = 0; i < scored.Count; i++)
        {
            contextBlock.AppendLine($"[{i + 1}] {scored[i].Chunk.Content}");
            contextBlock.AppendLine();
        }

        var messages = new List<Microsoft.Extensions.AI.ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, $"Context:\n{contextBlock}\n\nQuestion: {question}")
        };

        var fullAnswer = new StringBuilder();
        await foreach (var update in chatClient.CompleteStreamingAsync(messages))
        {
            fullAnswer.Append(update.Text);
            yield return new { type = "token", text = update.Text };
        }

        var docIds = scored.Select(s => s.Chunk.DocumentId).Distinct().ToList();
        var docNames = await db.Documents
            .Where(d => docIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.FileName);

        yield return new
        {
            type = "citations",
            citations = scored.Select(s => new
            {
                fileName = docNames.GetValueOrDefault(s.Chunk.DocumentId, "Unknown"),
                chunkIndex = s.Chunk.ChunkIndex,
                score = s.Score
            })
        };
    }
}

