using System.Threading.Channels;
using AIKnowledgeAssistant.Api.Hubs;
using AIKnowledgeAssistant.Core.Enums;
using AIKnowledgeAssistant.Infrastructure.Data;
using AIKnowledgeAssistant.Infrastructure.Processing;

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

            var doc = await db.Documents.FindAsync(documentId);
            if (doc is null) continue;

            try
            {
                doc.Status = ProcessingStatus.Extracting;
                await db.SaveChangesAsync(stoppingToken);
                await notifier.NotifyStatusChangeAsync(doc.Id, ProcessingStatus.Extracting);
                await Task.Delay(1500, stoppingToken); // artificial delay so the status is visible in a demo

                var text = await extractor.ExtractAsync(doc.BlobUri);

                doc.Status = ProcessingStatus.Chunking;
                await db.SaveChangesAsync(stoppingToken);
                await notifier.NotifyStatusChangeAsync(doc.Id, ProcessingStatus.Chunking);
                await Task.Delay(1500, stoppingToken);

                var chunks = chunker.Chunk(text);

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
