using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Textify;
using OpenQA.Selenium.Chrome;

namespace WebsiteCrawler;

public static class WebsiteCrawler
{
    private static readonly HashSet<string> VisitedLinks = [];

    static WebsiteCrawler()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        var driver = new ChromeDriver(options);
        Driver = driver;
    }

    private static readonly ChromeDriver Driver;
    
    private static readonly HtmlToTextConverter Converter = new();
    
    private static readonly Dictionary<string, string> WebsiteInfo = new();
    
    public static async Task<Dictionary<string, string>> Crawl(string baseurl, int maxDepth, int curDepth = 0, string url = "")
    {
        if (string.IsNullOrEmpty(url)) url = baseurl;
        
        if (!VisitedLinks.Add(url) || (curDepth > maxDepth /*&& !url.Contains(baseurl)*/)) return WebsiteInfo;
        
        Console.WriteLine($"[WebsiteCrawler] MaxDepth: {maxDepth,-2} | Current Depth: {curDepth,-2} | Attempting to crawl into url: {url}");
        
        // Fetch HTML from URL using Selenium
        var html = "";
        try
        {
            await Driver.Navigate().GoToUrlAsync(url);
            html = Driver.PageSource;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebsiteCrawler] Could not access website, ran into error: {ex.Message}\n{ex}");
        }
        
        // Clean HTML into readable plaintext
        var plainText = Converter.Convert(html);
        
        // Get list of links from HTML, and format plaintext to have links inside of it
        var plaintextPlusLinks = FormatLinks(plainText, url, out var links);
        
        //Console.WriteLine(plaintextPlusLinks);
        WebsiteInfo[url] = plaintextPlusLinks;
        
        // Crawls into the links found on the webpage
        foreach (var link in links.Where(l => l.Contains("http")))
        {
            await Crawl(baseurl, maxDepth, curDepth + 1, link);
        }
        
        return WebsiteInfo;
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