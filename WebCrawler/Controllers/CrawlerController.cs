using Abot.Crawler;
using AbotX.Crawler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebCrawler.Controllers
{
    public class CrawlerController : ApiController
    {
        private List<string> imageUrls;
        private string baseUrl;
        private int exceptionCounter = 0;

        public IHttpActionResult Get(string url)
        {
            baseUrl = url = url.StartsWith("http") ? url : $"http://{url}";

            var crawler = new CrawlerX();

            var uri = new Uri(url);

            crawler.PageCrawlCompleted += crawler_ProcessPageCrawlCompleted;

            var result = crawler.Crawl(uri);

            if (result.ErrorOccurred || exceptionCounter > 0)
            {
                throw new Exception($"Error occured while saving Images from {url}");
            }

            return Json(imageUrls);
        }

        void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            var images = e.CrawledPage.AngleSharpHtmlDocument.Images;

            imageUrls = images.Select(image => image.Source).ToList();
            var validImgUrls = imageUrls.Where(x => x.StartsWith("http")).ToList();
            var invalidImgUrls = imageUrls.Where(x => !x.StartsWith("http"));

            try
            {
                ValidateImageUrls(invalidImgUrls).ForEach(async url => await SaveImageAsync(url));
            }
            catch (Exception)
            {
                exceptionCounter++;
            }

            try
            {
                validImgUrls.ForEach(async url => await SaveImageAsync(url));
            }
            catch (Exception)
            {
                exceptionCounter++;
            }
        }

        private async Task SaveImageAsync(string url)
        {
            var data = await DownloadFileAsync(url);
            var readerWriterLockSlim = new ReaderWriterLockSlim();
            try
            {
                readerWriterLockSlim.EnterWriteLock();

                Directory.CreateDirectory("C:/Images");

                var fileName = $"C:/Images/{Regex.Replace(url, ".*/", string.Empty)}";
                
                File.WriteAllBytes(fileName, data);
            }
            catch (Exception ex)
            {
                exceptionCounter++;
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        private async Task<byte[]> DownloadFileAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetByteArrayAsync(url);
            }
        }

        private List<string> ValidateImageUrls(IEnumerable<string> imgUrls)
        {
            var validUrls = new List<string>();

            foreach (var url in imgUrls)
            {
                var numberOfDots = url.Where(x => x == '.');

                var result = default(string);

                if (numberOfDots.Count() > 0)
                {
                    result = Regex.Replace(url, ".*//", "http://");
                }
                else
                {
                    result = Regex.Replace(url, ".*://", baseUrl);
                }

                validUrls.Add(result);
            }

            return validUrls;
        }
    }
}
