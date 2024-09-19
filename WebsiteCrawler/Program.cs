namespace WebsiteCrawler;

internal abstract class Program
{
    private static async Task Main(string[] args)
    {
        const string url = "https://www.scrapingcourse.com/ecommerce/";
        const int maxDepth = 0;
        
        await WebsiteCrawler.Crawl(url, maxDepth);
        
        Console.WriteLine("\nCrawling complete!");
    }
}