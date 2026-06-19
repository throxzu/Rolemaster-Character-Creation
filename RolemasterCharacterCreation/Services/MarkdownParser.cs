using System.Text;

namespace RolemasterCharacterCreation.Services;

public static class MarkdownParser
{
    public static IReadOnlyList<(string Heading, string Text)> ParseChunks(string mdPath, int maxChunkWords)
    {
        var lines = File.ReadAllLines(mdPath, Encoding.UTF8);
        var chunks = new List<(string, string)>();
        var buffer = new StringBuilder();
        var currentHeading = Path.GetFileNameWithoutExtension(mdPath);
        var inFrontmatter = false;
        var frontmatterDone = false;
        var lineIndex = 0;

        foreach (var raw in lines)
        {
            lineIndex++;
            var line = raw.TrimEnd();

            // Skip YAML frontmatter
            if (lineIndex == 1 && line == "---") { inFrontmatter = true; continue; }
            if (inFrontmatter) { if (line == "---") { inFrontmatter = false; frontmatterDone = true; } continue; }

            // Heading detection
            if (line.StartsWith('#'))
            {
                var headingText = line.TrimStart('#').Trim();
                if (string.IsNullOrWhiteSpace(headingText)) continue;

                Flush(chunks, buffer, currentHeading);
                buffer.Clear();
                currentHeading = headingText;
                continue;
            }

            // Content line — skip blank lines that separate structure but keep table separators
            if (string.IsNullOrWhiteSpace(line))
            {
                // Preserve a single space so words don't run together across blank lines
                if (buffer.Length > 0 && buffer[^1] != ' ')
                    buffer.Append(' ');
                continue;
            }

            // Split if chunk is too large
            if (WordCount(buffer) + WordCount(line) > maxChunkWords && buffer.Length > 0)
            {
                Flush(chunks, buffer, currentHeading);
                buffer.Clear();
                currentHeading += " (cont.)";
            }

            buffer.Append(line).Append(' ');
        }

        Flush(chunks, buffer, currentHeading);
        return chunks;
    }

    private static void Flush(List<(string, string)> chunks, StringBuilder buffer, string heading)
    {
        var text = buffer.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(text))
            chunks.Add((heading, text));
    }

    private static int WordCount(StringBuilder sb) =>
        sb.Length == 0 ? 0 : sb.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    private static int WordCount(string s) =>
        string.IsNullOrWhiteSpace(s) ? 0 : s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
}
