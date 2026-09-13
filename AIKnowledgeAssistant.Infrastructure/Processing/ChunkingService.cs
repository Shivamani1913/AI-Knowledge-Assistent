namespace AIKnowledgeAssistant.Infrastructure.Processing;

public record TextChunk(int Index, string Content);

public class ChunkingService
{
    public List<TextChunk> Chunk(string text, int maxWordsPerChunk = 350, int overlapWords = 50)
    {
        var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<TextChunk>();

        if (words.Length == 0)
        {
            return chunks;
        }

        var index = 0;
        var start = 0;

        while (start < words.Length)
        {
            var length = Math.Min(maxWordsPerChunk, words.Length - start);
            var chunkWords = words.Skip(start).Take(length);
            chunks.Add(new TextChunk(index, string.Join(' ', chunkWords)));

            index++;
            start += maxWordsPerChunk - overlapWords;
        }

        return chunks;
    }
}
