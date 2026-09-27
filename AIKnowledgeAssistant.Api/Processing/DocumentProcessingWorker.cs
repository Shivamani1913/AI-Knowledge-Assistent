using System.Text.Json;
using System.Threading.Channels;
using AIKnowledgeAssistant.Api.Hubs;
using AIKnowledgeAssistant.Core.Entities;
using AIKnowledgeAssistant.Core.Enums;
using AIKnowledgeAssistant.Infrastructure.Data;
using AIKnowledgeAssistant.Infrastructure.Processing;
using Microsoft.Extensions.AI;

namespace AIKnowledgeAssistant.Api.Processing;

public class DocumentProcessingWorker(
    Channel<Guid> queue,
    IServiceScopeFactory scopeFactory,
    IProcessingNotifier notifier) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var documentId in queue.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var extractor = scope.ServiceProvider.GetRequiredService<DocumentTextExtractor>();
            var chunker = scope.ServiceProvider.GetRequiredService<ChunkingService>();
            var embeddingGenerator = scope.ServiceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

            var doc = await db.Documents.FindAsync(documentId);
            if (doc is null) continue;

            try
            {
                doc.Status = ProcessingStatus.Extracting;
                await db.SaveChangesAsync(stoppingToken);
                await notifier.NotifyStatusChangeAsync(doc.Id, ProcessingStatus.Extracting);

                var text = await extractor.ExtractAsync(doc.BlobUri);

                doc.Status = ProcessingStatus.Chunking;
                await db.SaveChangesAsync(stoppingToken);
                await notifier.NotifyStatusChangeAsync(doc.Id, ProcessingStatus.Chunking);

                var chunks = chunker.Chunk(text);

                doc.Status = ProcessingStatus.Embedding;
                await db.SaveChangesAsync(stoppingToken);
                await notifier.NotifyStatusChangeAsync(doc.Id, ProcessingStatus.Embedding);

                foreach (var chunk in chunks)
                {
                    var embeddingResult = await embeddingGenerator.GenerateAsync(new[] { chunk.Content });
                    var vector = embeddingResult.First().Vector.ToArray();

                    db.Chunks.Add(new DocumentChunk
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = doc.Id,
                        ChunkIndex = chunk.Index,
                        Content = chunk.Content,
                        EmbeddingJson = JsonSerializer.Serialize(vector)
                    });
                }

                doc.ChunkCount = chunks.Count;
                doc.Status = ProcessingStatus.Completed;
                await db.SaveChangesAsync(stoppingToken);
                await notifier.NotifyStatusChangeAsync(doc.Id, ProcessingStatus.Completed);
            }
            catch (Exception ex)
            {
                doc.Status = ProcessingStatus.Failed;
                doc.ErrorMessage = ex.Message;
                await db.SaveChangesAsync(stoppingToken);
                await notifier.NotifyStatusChangeAsync(doc.Id, ProcessingStatus.Failed, ex.Message);
            }
        }
    }
}

