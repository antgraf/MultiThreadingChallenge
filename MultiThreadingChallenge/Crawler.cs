using System.Collections.Concurrent;

namespace MultiThreadingChallenge
{
    internal class Crawler : ICrawler
    {
        #region ICrawler

        public Func<string, CancellationToken, Task<string>> Downloader { get; set; } = (url, cancellationToken) => Task.FromResult(string.Empty);

        public Func<string, IEnumerable<string>> Parser { get; set; } = html => [];

        public async Task<IEnumerable<string>> CrawlAsync(IEnumerable<string> urls, int concurrency, CancellationToken cancellationToken)
        {
            ConcurrentDictionary<string, bool> collected = [];
            SemaphoreSlim semaphore = new(concurrency);
            ConcurrentDictionary<Task<string>, bool> tasks = new();
            CrawlAsync(urls, tasks, collected, semaphore, cancellationToken);
            while (!tasks.IsEmpty)
            {
                Task<string> task = await Task.WhenAny(tasks.Keys);
                tasks.Remove(task, out _);
                string html = await task;
                IEnumerable<string> newUrls = Parser(html);
                CrawlAsync(newUrls, tasks, collected, semaphore, cancellationToken);
            }
            return collected.Keys;
        }

        #endregion

        private void CrawlAsync(IEnumerable<string> urls, ConcurrentDictionary<Task<string>, bool> tasks, ConcurrentDictionary<string, bool> collected,
            SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            foreach (var url in urls)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!collected.TryAdd(url, false))
                {
                    continue;
                }
                tasks.TryAdd(DownloadPageAsync(url, semaphore, cancellationToken), false);
            }
        }

        private async Task<string> DownloadPageAsync(string url, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await Downloader(url, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
