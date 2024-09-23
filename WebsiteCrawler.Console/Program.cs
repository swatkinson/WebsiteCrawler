namespace WebsiteCrawler.Console;

internal abstract class Program
{
    private static async Task Main(string[] args)
    {
        const string url = "https://www.scrapingcourse.com/ecommerce/";
        const int maxDepth = 1;

        var websiteInfo = await WebsiteCrawler.Crawl(url, maxDepth);

        foreach (var website in websiteInfo.Keys)
        {
            RandomConsoleColour();
            System.Console.WriteLine($"Website: {website} \n {websiteInfo[website]}");
        }
        
        System.Console.WriteLine("\nCrawling complete!");
    }
    
    private static void RandomConsoleColour()
    {
        var random = new Random();
        var colors = Enum.GetValues(typeof(ConsoleColor));
        ConsoleColor randomColor;
        do
        {
            randomColor = (ConsoleColor)colors.GetValue(random.Next(colors.Length))!;
        } while (randomColor == ConsoleColor.Black || randomColor == ConsoleColor.DarkGray || randomColor == System.Console.ForegroundColor);

        System.Console.ForegroundColor = randomColor;
    }
}