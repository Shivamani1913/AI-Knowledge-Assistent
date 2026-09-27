using AIKnowledgeAssistant.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIKnowledgeAssistant.Api.Controllers;

public record AskRequest(string Question);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(IRagService ragService) : ControllerBase
{
    [HttpPost("ask")]
    public async Task<IActionResult> Ask(AskRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var sessionId = Guid.NewGuid();

        var fullAnswer = new System.Text.StringBuilder();
        object? citations = null;

        await foreach (var chunk in ragService.StreamAnswerAsync(sessionId, userId, request.Question))
        {
            var json = System.Text.Json.JsonSerializer.Serialize(chunk);
            var doc = System.Text.Json.JsonDocument.Parse(json);

            if (doc.RootElement.GetProperty("type").GetString() == "token")
            {
                fullAnswer.Append(doc.RootElement.GetProperty("text").GetString());
            }
            else if (doc.RootElement.GetProperty("type").GetString() == "citations")
            {
                citations = chunk;
            }
        }

        return Ok(new { answer = fullAnswer.ToString(), citations });
    }
}
