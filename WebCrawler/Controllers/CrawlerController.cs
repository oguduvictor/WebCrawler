using Abot.Crawler;
using Abot.Poco;
using AbotX.Crawler;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebCrawler.Controllers
{
    public class CrawlerController : ApiController
    {
        private PageContent pageContent;
        
        public IHttpActionResult Get()
        {
            var crawler = new CrawlerX();
            
            crawler.PageCrawlCompleted += crawler_ProcessPageCrawlCompleted;

            var uri = new Uri("http://nairaland.com");

            var result = crawler.Crawl(uri);

            return Json(pageContent);
        }

        void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPageResult = e.CrawledPage;

            pageContent = crawledPageResult.Content;
        }
    }
}
