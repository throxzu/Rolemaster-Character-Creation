using System.Text;
using HtmlAgilityPack;

namespace RolemasterCharacterCreation.Services;

public static class RulesParser
{
    private static readonly HashSet<string> HeadingTags = ["h1", "h2", "h3", "h4"];
    private static readonly HashSet<string> ContentTags = ["p", "li", "td", "th", "dt", "dd"];

    public static IReadOnlyList<(string Heading, string Text)> ParseChunks(string htmlPath, int maxChunkWords)
    {
        var doc = new HtmlDocument();
        doc.Load(htmlPath, Encoding.UTF8);

        var root = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;
        var chunks = new List<(string Heading, string Text)>();
        var buffer = new StringBuilder();
        var currentHeading = "Introduction";
        var inToc = false;

        foreach (var node in root.DescendantsAndSelf())
        {
            if (node.NodeType != HtmlNodeType.Element) continue;

            var tag = node.Name.ToLowerInvariant();

            if (HeadingTags.Contains(tag))
            {
                var headingText = HtmlEntity.DeEntitize(node.InnerText).Trim();
                if (string.IsNullOrWhiteSpace(headingText)) continue;

                if (headingText.Contains("table of contents", StringComparison.OrdinalIgnoreCase))
                {
                    inToc = true;
                }
                else if (tag is "h1" or "h2")
                {
                    inToc = false;
                }

                Flush(chunks, buffer, currentHeading);
                buffer.Clear();
                currentHeading = headingText;
            }
            else if (!inToc && ContentTags.Contains(tag))
            {
                // Skip nodes that are containers for other content nodes to avoid double-counting
                if (node.ChildNodes.Any(c => ContentTags.Contains(c.Name.ToLowerInvariant())))
                    continue;

                var text = HtmlEntity.DeEntitize(node.InnerText).Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                // Split oversized chunks at word boundaries
                if (WordCount(buffer) + WordCount(text) > maxChunkWords && buffer.Length > 0)
                {
                    Flush(chunks, buffer, currentHeading);
                    buffer.Clear();
                    currentHeading += " (cont.)";
                }

                buffer.Append(text).Append(' ');
            }
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

    private static int WordCount(string text) =>
        string.IsNullOrWhiteSpace(text) ? 0 : text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
}
