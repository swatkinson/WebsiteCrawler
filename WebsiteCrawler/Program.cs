namespace WebsiteCrawler;

internal abstract class Program
{
    private static async Task Main(string[] args)
    {
        const string url = "https://www.scrapingcourse.com/ecommerce/";
        const int maxDepth = 1;
        Console.WriteLine($"Crawling at head URL {url} to a depth of {maxDepth}...\n");

        await WebsiteCrawler.Crawl(url, 0, maxDepth);
        
        Console.WriteLine("\nCrawling complete!");
    }
}