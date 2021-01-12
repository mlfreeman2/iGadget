using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangfire;
using Hangfire.SQLite;
using iGadget.DB;
using iGadget.App.Tasks.PublixAd;
using iGadget.App.Tasks.RSS;
using iGadget.App.Tasks.Weather;
using iGadget.App.Tasks.Comics;
using Hangfire.Dashboard;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace iGadget.App
{
    public class Startup
    {

        private readonly IWebHostEnvironment _env;

        private readonly IConfiguration _config;

        public Startup(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var hangfireDatabasePath = _config.GetConnectionString("Hangfire");

            var sqliteOptions = new SQLiteStorageOptions();

            services.AddControllers().AddNewtonsoftJson();

            services.AddHangfire((provider, configuration) => {
                configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage(hangfireDatabasePath, sqliteOptions);
            });

            services.AddDbContext<iGadgetDataCache>(options => options.UseSqlite(_config.GetConnectionString("iGadgetDataCache")));
        }

        public void Configure(IApplicationBuilder app, IBackgroundJobClient backgroundJobs, IWebHostEnvironment env, iGadgetDataCache dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseHangfireServer(new BackgroundJobServerOptions { WorkerCount = 1 });
            app.UseFileServer(new FileServerOptions {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "wwwroot")),
                RequestPath = "/iGadget"
            });
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard("/hangfire1", new DashboardOptions {
                    Authorization = new IDashboardAuthorizationFilter[0],  
                });
            });

            dbContext.Database.EnsureCreated();

            foreach (var feedEntry in _config.GetSection("RSSFeeds").GetChildren())
            {
                var rssFeed = new Models.Config.RSS.RSSFeed();
                feedEntry.Bind(rssFeed);
                RecurringJob.AddOrUpdate<RSSTasks>(rssFeed.Name, a => a.DownloadRSS(rssFeed), rssFeed.CronSchedule, TimeZoneInfo.Local);
            }

            foreach (var weatherSiteEntry in _config.GetSection("NationalWeatherService:Sites").GetChildren())
            {
                var weatherSite = new Models.Config.NWSSite();
                weatherSiteEntry.Bind(weatherSite);

                RecurringJob.AddOrUpdate<NWSTasks>($"{weatherSite.Name} - Current Observations", a => a.GetWeatherObservations(weatherSite), weatherSite.ObservationSchedule, TimeZoneInfo.Local);
                RecurringJob.AddOrUpdate<NWSTasks>($"{weatherSite.Name} - Forecast", a => a.GetWeatherForecast(weatherSite), weatherSite.ForecastSchedule, TimeZoneInfo.Local);
            }

            foreach (var publixAdEntry in _config.GetSection("PublixAds:Stores").GetChildren())
            {
                var publixAd = new Models.Config.PublixAd();
                publixAdEntry.Bind(publixAd);
                RecurringJob.AddOrUpdate<PublixWeeklyAdTasks>($"Publix Weekly Ad - ZIP Code {publixAd.ZIPCode}", a => a.DownloadWeeklyAd(publixAd), publixAd.Schedule, TimeZoneInfo.Local);
            }

            foreach (var comicEntry in _config.GetSection("Comics").GetChildren())
            {
                var comic = new Models.Config.Comic();
                comicEntry.Bind(comic);
                RecurringJob.AddOrUpdate<ComicTasks>(comic.Name, a => a.DownloadComic(comic), comic.Schedule, TimeZoneInfo.Local);
            }

            var inPlay = new Models.Config.InPlay();
            _config.GetSection("InPlay").Bind(inPlay);
            RecurringJob.AddOrUpdate<InPlayTasks>("InPlay", a => a.DownloadInPlay(inPlay), inPlay.Schedule, TimeZoneInfo.Local);
        }
    }
}
