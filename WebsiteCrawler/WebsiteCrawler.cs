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
    
    public static async Task Crawl(string url, int maxDepth, int? urlNumber = null, int curDepth = 0)
    {
        if (curDepth > maxDepth || !_visitedLinks.Add(url))
        {
            return;
        }

        RandomConsoleColour();
        
        Console.WriteLine(urlNumber is null
            ? $"Crawling into base url \"{url}\" to a maximum depth of {maxDepth}..."
            : $"\n\n\nAttempting to crawl into link [{urlNumber}] {url}.");
        
        var webClient = new HttpClient();

        webClient.Timeout = TimeSpan.FromSeconds(5);
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Add headers to mimic a real browser
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Referer", "http://www.google.com/");
        
        var response = await webClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        
        var converter = new HtmlToTextConverter();
        var cleanHtml = converter.Convert(html);

        var httpLinks = ExtractLinks(cleanHtml);
        
        Console.WriteLine(cleanHtml);
        
        foreach (var link in httpLinks.Where(l => l.Value.Contains("http")))
        {
            try
            {
                await Crawl(link.Value, maxDepth, link.Key, curDepth + 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not access website, ran into error: {ex.Data}\n{ex.Message}");
            }
            
        }
    }

    private static Dictionary<int, string> ExtractLinks(string cleanHtml)
    {
        var links = new Dictionary<int, string>();
        foreach (Match match in LinkRegex().Matches(cleanHtml))
        {
            links.Add(int.Parse(match.Groups[1].Value), match.Groups[2].Value);
        }
        
        return links;
    }

    private static void RandomConsoleColour()
    {
        var random = new Random();
        var colors = Enum.GetValues(typeof(ConsoleColor));
        ConsoleColor randomColor;
        do
        {
            randomColor = (ConsoleColor)colors.GetValue(random.Next(colors.Length))!;
        }
        while (randomColor == ConsoleColor.Black|| randomColor == ConsoleColor.DarkGray || randomColor == Console.ForegroundColor);
        Console.ForegroundColor = randomColor;
    }
}