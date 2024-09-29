namespace MultiThreadingChallenge
{
    internal interface ICrawler
    {
        Func<string, CancellationToken, Task<string>> Downloader { get; }
        Func<string, IEnumerable<string>> Parser { get; }

        Task<IEnumerable<string>> CrawlAsync(IEnumerable<string> urls, int concurrency, CancellationToken cancellationToken);
    }
}
