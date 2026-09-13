namespace AIKnowledgeAssistant.Core.Interfaces;

public interface IRagService
{
    IAsyncEnumerable<object> StreamAnswerAsync(Guid sessionId, string userId, string question);
}
