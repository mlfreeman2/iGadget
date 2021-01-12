using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AngleSharp;
using Hangfire;
using iGadget.App.Util;
using iGadget.DB;
using iGadget.DB.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using iGadget.App.Tasks.Shared;

namespace iGadget.App.Tasks.Comics 
{
    public class InPlayTasks 
    {
        private IServiceProvider _serviceProvider;

        private Microsoft.Extensions.Configuration.IConfiguration _config;

        private IBackgroundJobClient _client;

        public InPlayTasks(IServiceProvider sp, IBackgroundJobClient client)
        {
            _serviceProvider = sp;
            _client = client;
            _config = _serviceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
        }

        [ObjectFriendlyJobDisplayName("Fetch daily InPlay Briefing")]
        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 10, 90 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task DownloadInPlay(Models.Config.InPlay inPlay)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            using (var dbContext = scope.ServiceProvider.GetRequiredService<iGadgetDataCache>())
            {
                var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithDefaultCookies());

                var page = await context.OpenAsync(inPlay.URL.ToString());
                var element = page.QuerySelector(inPlay.Selector);

                if (element == null) 
                {
                    throw new InvalidOperationException($"The InPlay selector '{inPlay.Selector}' found no elements on {inPlay.URL}");
                }
                
                var subject = $"{element.Owner.Title} for {DateTime.Now.ToShortDateString()}";
                var body = $"<html><head><title>{subject}</title></head><body>{element.OuterHtml}</body></html>";

                var email = new EMail {
                    From = _config["InPlay:EMailFrom"],
                    Bcc = inPlay.Recipients.Select(a => new EMailRecipient { Address = a }).ToList(),
                    Subject = subject,
                    HTMLBody = body
                };
                
                var dbInPlay = new InPlayDailyBriefing {
                    DownloadDate = DateTime.Now,
                    Title = subject,
                    HTML = body,
                    EMail = email
                };
                
                dbContext.InPlayDailyBriefings.Add(dbInPlay);
                dbContext.Entry(dbInPlay).State = EntityState.Added;

                dbContext.EMails.Add(email);
                dbContext.Entry(email).State = EntityState.Added;
                
                dbContext.SaveChanges();

                _client.Enqueue<SendEMail>(a => a.SendAnEMail(email.ID));
            }
        }
    }
}