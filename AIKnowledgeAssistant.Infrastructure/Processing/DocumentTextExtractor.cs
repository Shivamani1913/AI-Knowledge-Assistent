using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace AIKnowledgeAssistant.Infrastructure.Processing;

public class DocumentTextExtractor
{
    public Task<string> ExtractAsync(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => Task.FromResult(ExtractFromPdf(filePath)),
            ".docx" => Task.FromResult(ExtractFromDocx(filePath)),
            ".txt" => File.ReadAllTextAsync(filePath),
            _ => throw new NotSupportedException($"Unsupported file type: {extension}")
        };
    }

    private static string ExtractFromPdf(string filePath)
    {
        using var document = PdfDocument.Open(filePath);
        var text = new System.Text.StringBuilder();
        foreach (var page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }
        return text.ToString();
    }

    private static string ExtractFromDocx(string filePath)
    {
        using var document = WordprocessingDocument.Open(filePath, false);
        var body = document.MainDocumentPart?.Document.Body;
        return body?.InnerText ?? string.Empty;
    }
}
