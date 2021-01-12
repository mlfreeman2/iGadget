using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using iGadget.DB;
using iGadget.DB.Models;
using iGadget.App.Util;

namespace iGadget.App.Tasks.RSS
{
    public class RSSTasks
    {
        private IServiceProvider _serviceProvider;
        public RSSTasks(IServiceProvider sp)
        {
            _serviceProvider = sp;
        }

        [ObjectFriendlyJobDisplayName("Fetch {0:Count} RSS items from {0:Name}")]
        [AutomaticRetry(Attempts = 1, DelaysInSeconds = new int[] { 10 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task DownloadRSS(Models.Config.RSS.RSSFeed feed)
        {
            var content = await WebClient.GetString(feed.URL, "application/atom+xml,application/rdf+xml,application/rss+xml,application/x-netcdf,application/xml;q=0.9,text/xml;q=0.2,*/*;q=0.1");

            var items = new List<RSSFeedItem>();

            try
            {
                using (var reader = XmlReader.Create(new StringReader(content)))
                {
                    var parsedFeed = SyndicationFeed.Load(reader);

                    for (int i = 0; i < Math.Min(parsedFeed.Items.Count(), feed.Count); i++)
                    {
                        var item = parsedFeed.Items.ElementAt(i);

                        var myFeedItem = new RSSFeedItem();

                        myFeedItem.Title = item.Title.Text;
                        if (item.Links != null && item.Links.Any())
                        {
                            myFeedItem.Link = item.Links.First().GetAbsoluteUri();
                        }
                        items.Add(myFeedItem);
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to fetch RSS feed for {feed.Name} from {feed.URL}.", e);
            }

            if (items.Any())
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                using (var dbContext = scope.ServiceProvider.GetRequiredService<iGadgetDataCache>())
                {
                    var dbFeed = dbContext.Feeds.Include(a => a.Items).FirstOrDefault<RSSFeed>(a => a.Name == feed.Name);
                    if (dbFeed == null)
                    {
                        dbFeed = new RSSFeed {
                            Count = feed.Count,
                            Name = feed.Name,
                            Site = feed.Site,
                            URL = feed.URL,
                            Items = items
                        };
                        dbContext.Feeds.Add(dbFeed);
                        dbContext.SaveChanges();
                        return;
                    }

                    if (dbFeed.Items.Any())
                    {
                        dbContext.RemoveRange(dbFeed.Items);
                        dbFeed.Items.ForEach(a => dbContext.Entry(a).State = Microsoft.EntityFrameworkCore.EntityState.Deleted);
                        dbFeed.Items.Clear();
                        dbContext.SaveChanges();
                    }
                    dbContext.AddRange(items);
                    dbFeed.Items.AddRange(items);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}