using AIKnowledgeAssistant.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AIKnowledgeAssistant.Api.Hubs;

[Authorize]
public class ChatHub(IRagService ragService) : Hub
{
    public async Task AskQuestion(Guid sessionId, string question)
    {
        var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        await foreach (var chunk in ragService.StreamAnswerAsync(sessionId, userId, question))
        {
            await Clients.Caller.SendAsync("ReceiveAnswerChunk", chunk);
        }
    }
}
