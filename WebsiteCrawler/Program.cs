using Pinaka.Core;

namespace WebsiteCrawler;

internal abstract class Program
{
    private static async Task Main(string[] args)
    {
        PinakaParameters arguments = PinakaParameters.Parse(args);

        // Check if 'help' argument is passed and output help string
        // If Zero arguments are passed in then output help string
        if (PinakaParameters.RawValues.Count == 0 || arguments.Help)
        {
            PinakaParameters.PrintHelp();
            return; // Exit the application
        }

        // Example: Check if a specific argument is defined
        if (arguments.Verbose)
        {
            Console.WriteLine("Verbose mode enabled.");
        }

        // Example: Retrieve the value of a named argument
        if (!string.IsNullOrWhiteSpace(arguments.Output))
        {
            Console.WriteLine($"Output file: {arguments.Location}");
        }

        //const string url =
            // "https://www.scrapingcourse.com/ecommerce/";
        //    "https://www.resqwest.com";
        const int maxDepth = 1;
        
        await WebsiteCrawler.Crawl(arguments.URL, maxDepth);
        
        Console.WriteLine("\nCrawling complete!");
        Console.ReadLine();
    }
}

/*
 * TODO:
 * Fix accessing hotel website
 * Fix recursive calls
 * Add turn on/off main link crawl
 */