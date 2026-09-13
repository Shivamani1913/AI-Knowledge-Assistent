using AIKnowledgeAssistant.Core.Enums;
using Microsoft.AspNetCore.SignalR;

namespace AIKnowledgeAssistant.Api.Hubs;

public interface IProcessingNotifier
{
    Task NotifyStatusChangeAsync(Guid documentId, ProcessingStatus status, string? detail = null);
}

public class ProcessingNotifier(IHubContext<ProcessingHub> hub) : IProcessingNotifier
{
    public Task NotifyStatusChangeAsync(Guid documentId, ProcessingStatus status, string? detail = null) =>
        hub.Clients.Group("admins").SendAsync("DocumentStatusChanged", new
        {
            documentId,
            status = status.ToString(),
            detail,
            timestamp = DateTimeOffset.UtcNow
        });
}
