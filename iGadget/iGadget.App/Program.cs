using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace iGadget.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) => {
                    config.AddJsonFile("Config/rssFeeds.json");
                    config.AddJsonFile("Config/nwsSites.json");
                    config.AddJsonFile("Config/publixAds.json");
                    config.AddJsonFile("Config/eMailAccounts.json");
                    config.AddJsonFile("Config/comics.json");
                    config.AddJsonFile("Config/inPlay.json");
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
