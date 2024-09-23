# 🌐 WebsiteCrawler

WebsiteCrawler is a C# console application that recursively crawls websites, extracts links, and displays the text content of web pages.

## Features

- **Recursive Crawling**: Crawl websites up to a specified depth.
- **Link Extraction**: Extract and format links from web pages.
- **HTML to Text Conversion**: Convert HTML content to plain text.
- **Custom User-Agent**: Mimic a real browser by setting custom headers.

## Usage

1. **Set the Starting URL**: In the `Program` class, modify the `url` constant.

   ```csharp
   const string url = "https://www.example.com";
   ```

2. **Set the Maximum Depth**: Adjust the `maxDepth` constant.

   ```csharp
   const int maxDepth = 1;
   ```

3. **Call the Crawl Command**:

   ```bash
   await WebsiteCrawler.Crawl(url, maxDepth);
   ```

## Requirements

- .NET SDK
- `Textify` NuGet package
  
