using System.Text.RegularExpressions;
using Textify;

namespace WebsiteCrawler;

public static partial class WebsiteCrawler
{
    private static HashSet<string> _visitedLinks;
    
    static WebsiteCrawler()
    {
        _visitedLinks = new HashSet<string>();
    }

    [GeneratedRegex(@"\[(\d+)\] (http\S+)")]
    private static partial Regex LinkRegex();
    
    public static async Task Crawl(string baseurl, int maxDepth, int curDepth = 0, string url = "")
    {
        if (string.IsNullOrEmpty(url)) url = baseurl;
        
        if (!_visitedLinks.Add(url) || (curDepth > maxDepth && !url.Contains(baseurl))) return;

        RandomConsoleColour();

        Console.WriteLine($"\nAttempting to crawl into url \"{url}\" to a maximum depth of {maxDepth}, current depth is {curDepth}...");
        
        var webClient = new HttpClient();

        webClient.Timeout = TimeSpan.FromSeconds(5);
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Add headers to mimic a real browser
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Referer", "http://www.google.com/");

        var html = "";
        try
        {
            var response = await webClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            html = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not access website, ran into error: {ex.Message}\n{ex}");
        }
        
        var converter = new HtmlToTextConverter();
        var cleanHtml = converter.Convert(html);
        
        Console.WriteLine(FormatLinks(cleanHtml, url, out var links));
        
        foreach (var link in links)
        {
            await Crawl(baseurl, maxDepth, curDepth + 1, link);
        }
    }

    private static void RandomConsoleColour()
    {
        var random = new Random();
        var colors = Enum.GetValues(typeof(ConsoleColor));
        ConsoleColor randomColor;
        do
        {
            randomColor = (ConsoleColor)colors.GetValue(random.Next(colors.Length))!;
        } while (randomColor == ConsoleColor.Black || randomColor == ConsoleColor.DarkGray || randomColor == Console.ForegroundColor);

        Console.ForegroundColor = randomColor;
    }

    private static string FormatLinks(string input, string baseUrl, out List<string> collectedLinks)
    {
        collectedLinks = new List<string>();

        // Split the input text into lines
        var lines = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Dictionary to store footnote numbers and their corresponding URLs
        var links = new Dictionary<int, string>();

        // We'll store the index where the footnotes start
        int footnoteStartIndex = lines.Length;

        // Regular expression to match footnote lines like: [number] URL
        var footnotePattern = new Regex(@"^\s*\[(\d+)\]\s*(\S+)\s*$");

        // Iterate over the lines from the end backwards
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i];

            // Trim the line to remove any leading/trailing whitespace
            var trimmedLine = line.Trim();

            // Check if the line matches the footnote pattern
            var match = footnotePattern.Match(trimmedLine);

            if (match.Success)
            {
                // Line is a footnote link, add it to the dictionary
                var number = int.Parse(match.Groups[1].Value);
                var link = match.Groups[2].Value;

                // Prepend base URL if link starts with '/'
                if (link.StartsWith("/"))
                {
                    if (link.StartsWith("/#"))
                    {
                        // It's a fragment identifier, keep it as is
                        links[number] = link;
                        // Do not add to collectedLinks to prevent recursive crawling
                    }
                    else
                    {
                        link = baseUrl.TrimEnd('/') + link;
                        links[number] = link;
                        collectedLinks.Add(link);
                    }
                }
                else
                {
                    // Link does not start with '/', use as is
                    links[number] = link;
                    collectedLinks.Add(link);
                }

                // Update the footnote start index
                footnoteStartIndex = i;
            }
            else if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                // If the line is empty, continue checking
                continue;
            }
            else
            {
                // Line is not a footnote link, stop processing
                break;
            }
        }

        // Remove the footnote section from the lines
        var mainTextLines = lines.Take(footnoteStartIndex);

        // Combine the main text lines back into a string
        var mainText = string.Join("\n", mainTextLines);

        // Now replace the [number] in the main text with the corresponding URLs
        var result = Regex.Replace(mainText, @"\[(\d+)\]", match =>
        {
            var number = int.Parse(match.Groups[1].Value);
            return links.ContainsKey(number) ? $"[{links[number]}]" : match.Value;
        });

        return result;
    }
}