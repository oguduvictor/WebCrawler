using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebCrawler.Startup))]

namespace WebCrawler
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
