using Xunit;

namespace MultiThreadingChallenge
{
    public class CrawlerTest
    {
        private const int Concurrency = 3;

        private static ICrawler CreateCrawler(IEnumerable<string> urls, CancellationToken cancellationToken)
        {
            Crawler crawler = new()
            {
                Downloader = async (url, cancellationToken) => { await Task.Delay(1000, cancellationToken); return "dummy"; },
                Parser = html => urls
            };
            return crawler;
        }

        [Fact]
        public async Task NoUrls()
        {
            CancellationTokenSource source = new();
            ICrawler crawler = CreateCrawler([], source.Token);
            var urls = await crawler.CrawlAsync([], Concurrency, source.Token);
            Assert.Empty(urls);
        }

        [Fact]
        public async Task OneUrl1()
        {
            const string url = "a";
            ICrawler crawler = CreateCrawler([url], CancellationToken.None);
            var urls = await crawler.CrawlAsync([url], Concurrency, CancellationToken.None);
            Assert.Single(urls);
            Assert.Equal(url, urls.First());
        }

        [Fact]
        public async Task OneUrl2()
        {
            const string url = "a";
            ICrawler crawler = CreateCrawler([], CancellationToken.None);
            var urls = await crawler.CrawlAsync([url], Concurrency, CancellationToken.None);
            Assert.Single(urls);
            Assert.Equal(url, urls.First());
        }

        [Fact]
        public async Task ManyUrls()
        {
            ICrawler crawler = CreateCrawler(["a", "b", "c", "d", "e", "f"], CancellationToken.None);
            var urls = await crawler.CrawlAsync(["1"], Concurrency, CancellationToken.None);
            Assert.Equal(7, urls.Count());
        }

        [Fact]
        public async Task NoConcurrency()
        {
            ICrawler crawler = CreateCrawler(["a", "b", "c", "d", "e", "f"], CancellationToken.None);
            var urls = await crawler.CrawlAsync(["1"], 1, CancellationToken.None);
            Assert.Equal(7, urls.Count());
        }

        [Fact]
        public async Task Cancel()
        {
            CancellationTokenSource source = new();
            ICrawler crawler = CreateCrawler(["a", "b", "c", "d", "e", "f"], source.Token);
            var task = crawler.CrawlAsync(["1"], Concurrency, source.Token);
            await Task.Delay(500, source.Token);

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                source.Cancel();
                await task;
            });
        }
    }
}
