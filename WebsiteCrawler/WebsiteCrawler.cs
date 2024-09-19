using System.Text;
using HtmlAgilityPack;

namespace WebsiteCrawler;

public static class WebsiteCrawler
{
    public static async Task Crawl(string url, int curDepth, int maxDepth)
    {
        if (curDepth > maxDepth) // || !_visitedLinks.Add(url)
            return;

        var webClient = new HttpClient();
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Add headers to mimic a real browser
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Referer", "http://www.google.com/");
        
        var response = await webClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        Console.WriteLine(html);
        Console.WriteLine(ExtractTextFromHtml(html));

        var cssLinks = ExtractLinks(html);
    
        foreach (var link in cssLinks)
        {
            Console.WriteLine(link);
        }
    }

    private static string ExtractTextFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Remove script and style tags
        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style");

        if (nodesToRemove == null) return ExtractTextWithStructure(doc.DocumentNode);

        foreach (var node in nodesToRemove)
        {
            node.Remove();
        }

        // Extract and return structured text
        return ExtractTextWithStructure(doc.DocumentNode);
    }

    private static string ExtractTextWithStructure(HtmlNode? node)
    {
        var sb = new StringBuilder();

        if (node == null) return string.Empty; // Ensure the node itself isn't null

        foreach (var childNode in node.ChildNodes)
        {
            if (childNode == null) continue; // Ensure each child node isn't null

            switch (childNode.Name)
            {
                case "h1" or "h2" or "h3" or "h4" or "h5" or "h6":
                    // Add new line and heading text
                    sb.AppendLine().AppendLine(childNode.InnerText.Trim()).AppendLine();
                    break;
                case "p":
                    // Paragraph text
                    sb.AppendLine(childNode.InnerText.Trim()).AppendLine();
                    break;
                case "ul" or "ol":
                {
                    // Handle lists (ul or ol)
                    var listItems = childNode.SelectNodes(".//li");
                    if (listItems != null)
                    {
                        foreach (var li in listItems)
                        {
                            if (li != null)
                            {
                                sb.AppendLine("- " + li.InnerText.Trim());
                            }
                        }
                    }

                    sb.AppendLine();
                    break;
                }
                case "li":
                    // Handle individual list item if not inside a ul/ol block
                    sb.AppendLine("- " + childNode.InnerText.Trim());
                    break;
                default:
                {
                    // Recursively extract text from child nodes
                    sb.Append(
                        // General text extraction for other nodes
                        childNode.HasChildNodes ? ExtractTextWithStructure(childNode) : childNode.InnerText.Trim());

                    break;
                }
            }
        }

        return sb.ToString();
    }
    
    private static List<string> ExtractLinks(string pageContent)
    {
        var links = new List<string>();
        var startIndex = 0;

        while (true)
        {
            startIndex = pageContent.IndexOf("<a", startIndex, StringComparison.Ordinal);
            if (startIndex == -1)
                break;

            var endIndex = pageContent.IndexOf(">", startIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                break;

            var tagContent = pageContent.Substring(startIndex, endIndex - startIndex + 1);

            var attributeStartIndex = tagContent.IndexOf("href=\"", StringComparison.Ordinal);
            if (attributeStartIndex != -1)
            {
                attributeStartIndex += 6;
                var attributeEndIndex = tagContent.IndexOf("\"", attributeStartIndex, StringComparison.Ordinal);
                if (attributeEndIndex != -1)
                {
                    var link = tagContent.Substring(attributeStartIndex, attributeEndIndex - attributeStartIndex);
                    links.Add(link);
                }
            }

            startIndex = endIndex + 1;
        }

        return links;
    }
}