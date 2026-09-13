using Microsoft.AspNetCore.Identity;

namespace AIKnowledgeAssistant.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
}
