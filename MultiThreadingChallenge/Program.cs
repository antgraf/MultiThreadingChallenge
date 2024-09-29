using MultiThreadingChallenge;

const string urlStr = "a";
Crawler crawler = new()
{
    Downloader = async (url, cancellationToken) => { await Task.Delay(1000, cancellationToken); return "dummy"; },
    Parser = html => [urlStr]
};
var urls = await crawler.CrawlAsync([urlStr], 3, CancellationToken.None);

foreach (var url in urls)
{
    Console.WriteLine(url);
}
