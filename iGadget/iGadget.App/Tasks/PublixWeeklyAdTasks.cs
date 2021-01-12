using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using iGadget.App.Tasks.Shared;
using iGadget.App.Util;
using iGadget.DB;
using iGadget.DB.Models;
using PublixDotCom.Promotions;
using PublixDotCom.StoreInfo;

namespace iGadget.App.Tasks.PublixAd
{
    public class PublixWeeklyAdTasks
    {
        private IServiceProvider _serviceProvider;

        private IConfiguration _config;

        private IBackgroundJobClient _client;

        public PublixWeeklyAdTasks(IServiceProvider sp, IBackgroundJobClient client)
        {
            _serviceProvider = sp;
            _client = client;
            _config = _serviceProvider.GetService<IConfiguration>();
        }

        [ObjectFriendlyJobDisplayName("Download Weekly Publix Ad For ZIP Code {0:ZIPCode}")]
        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 10, 90 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task DownloadWeeklyAd(Models.Config.PublixAd store)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            using (var dbContext = scope.ServiceProvider.GetRequiredService<iGadgetDataCache>())
            {
                var stores = await StoreList.Fetch(store.ZIPCode);
                
                if (stores == null || !stores.Any())
                {
                    throw new InvalidOperationException($"No stores found for zip code {store.ZIPCode}");
                }

                var ad = await Welcome.Fetch(stores[0]);

                if (dbContext.PublixWeeklyAds.Any(a => a.ZIPCode == store.ZIPCode && a.StartDate == ad.SaleStartDate && a.EndDate == ad.SaleEndDate))
                {
                    // already seen this ad. skip fetching the images
                    return;
                }
            
                var publixAd = new DB.Models.PublixWeeklyAd {
                    ZIPCode = store.ZIPCode,
                    DownloadedDate = DateTime.Now,
                    Recipients = store.Recipients.Select(a => new PublixWeeklyAdRecipient { Address = a }).ToList(),
                    Subject =  $"Publix Weekly Ad for {ad.SaleStartDate.ToString("MMM dd, yyyy")} to {ad.SaleEndDate.ToString("MMM dd, yyyy")}",
                    StartDate = ad.SaleStartDate,
                    EndDate = ad.SaleEndDate
                };

                var pageDownloadIds = new List<string>();
                foreach (var page in ad.Pages.OrderBy(a => a.Order))
                {
                    var content = await WebClient.GetFile(page.ImageUrl);
                    publixAd.AdPages.Add(new PublixWeeklyAdPage { PageNumber = page.Order, Image = new DownloadedImage(content) });
                } 


                var email = new EMail {
                    From = _config["PublixAds:EMailFrom"],
                    Bcc = publixAd.Recipients.Select(a => new EMailRecipient { Address = a.Address }).ToList(),
                    Subject = publixAd.Subject,
                    HTMLBody = $"<html><head><title>{publixAd.Subject}</title></head><body><h1>{publixAd.Subject}</h1>"
                };

                foreach (var page in publixAd.AdPages.OrderBy(a => a.PageNumber)) 
                {
                    var image = new EMailAttachment { FileName = $"{page.Image.RecommendedFileName}", Content = page.Image.Content, ContentDisposition = "inline", MIMEType = page.Image.MIMEType };
                    email.Attachments.Add(image);
                    email.HTMLBody += $"<img src=\"cid:{image.FileName}\" />";
                }
                email.HTMLBody += "</body></html>";

                publixAd.EMail = email;
                dbContext.PublixWeeklyAds.Add(publixAd);
                dbContext.EMails.Add(email);
                dbContext.Entry(email).State = EntityState.Added;
                dbContext.SaveChanges();

                _client.Enqueue<SendEMail>(a => a.SendAnEMail(email.ID));
            }
        }
    }
}