namespace AIKnowledgeAssistant.Web.Services;

public class AuthState
{
    public string? Token { get; set; }
    public string? Email { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
}
