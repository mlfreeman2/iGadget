using Microsoft.EntityFrameworkCore;
using iGadget.DB.Models;

namespace iGadget.DB
{
    public class iGadgetDataCache : DbContext
    {
        public iGadgetDataCache() { }

        public iGadgetDataCache(DbContextOptions<iGadgetDataCache> options) : base(options) { }

        public DbSet<WeatherSite> WeatherSites { get; set; }

        public DbSet<Comic> Comics { get; set; }

        public DbSet<RSSFeed> Feeds { get; set; }

        public DbSet<PublixWeeklyAd> PublixWeeklyAds { get; set; }

        public DbSet<InPlayDailyBriefing> InPlayDailyBriefings { get; set; }

        public DbSet<EMail> EMails { get; set; }
        
        public DbSet<DownloadedImage> Images { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PublixWeeklyAd>().HasIndex(c => new { c.ZIPCode, c.StartDate }).IsUnique();
        }
    }
}