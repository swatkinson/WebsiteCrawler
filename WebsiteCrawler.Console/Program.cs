namespace WebsiteCrawler.Console;

internal abstract class Program
{
    private static async Task Main(string[] args)
    {
        const string url =
            // "https://www.scrapingcourse.com/ecommerce/";
            "https://www.resqwest.com";
            //"https://www.hiltonwaikoloavillage.com/";
        const int maxDepth = 1;

        await WebsiteCrawler.Crawl(url, maxDepth);
        
        System.Console.WriteLine("\nCrawling complete!");
    }
}

/*
 * TODO:
 * Fix accessing hotel website
 * Fix recursive calls
 * Add turn on/off main link crawl
 */