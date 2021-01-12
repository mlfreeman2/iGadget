using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using iGadget.DB;
using iGadget.App.Util;
using AngleSharp;
using System.Linq;
using iGadget.DB.Models;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using AngleSharp.Html.Dom;

namespace iGadget.App.Tasks.Comics 
{
    public class ComicTasks 
    {
        private IServiceProvider _serviceProvider;

        public ComicTasks(IServiceProvider sp)
        {
            _serviceProvider = sp;
        }

        [ObjectFriendlyJobDisplayName("Fetch the latest issue of the comic '{0:Name}'")]
        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 10, 90 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task DownloadComic(Models.Config.Comic comic)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            using (var dbContext = scope.ServiceProvider.GetRequiredService<iGadgetDataCache>())
            {
                var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithDefaultCookies());
                var page = await context.OpenAsync(comic.URL.ToString());
                var element = page.QuerySelector(comic.Selector);

                if (element == null) 
                {
                    throw new InvalidOperationException($"The selector '{comic.Selector}' found no elements on {comic.Name} - {comic.URL}");
                }

                if (!(element is IHtmlImageElement))
                {
                    throw new InvalidOperationException($"The selector '{comic.Selector}' found non-image elements on {comic.Name} - {comic.URL}");
                }
                var imgTag = element as IHtmlImageElement;
                var alt = imgTag.AlternativeText;
                var title = imgTag.Title;
                var src = imgTag.Source;

                var dbComic = dbContext.Comics.Include(a => a.Comics).ThenInclude(a => a.Image).FirstOrDefault(a => a.Name == comic.Name);

                if (dbComic == null) 
                {
                    dbContext.Comics.Add(new DB.Models.Comic { Name = comic.Name, URL = comic.URL });
                    dbContext.SaveChanges();
                    dbComic = dbContext.Comics.Include(a => a.Comics).ThenInclude(a => a.Image).FirstOrDefault(a => a.Name == comic.Name);
                }

                if (!string.IsNullOrWhiteSpace(comic.Before) && !string.IsNullOrWhiteSpace(comic.After))
                {
                    src = Regex.Replace(src, comic.Before, comic.After);
                }

                var imgBytes = await WebClient.GetFile(new Uri(src));
                var image = new DownloadedImage(imgBytes);
                
                if (dbContext.Images.Any(a => a.MD5 == image.MD5)) 
                {
                    return;
                }

                var issue = new ComicIssue {
                    Alt = alt,
                    Title = title,
                    DownloadedDate = DateTime.Now,
                    RecommendedFileName = image.RecommendedFileName,
                    Image = image
                };
                dbComic.Comics.Add(issue);
                dbContext.Entry(issue).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                
                dbContext.Images.Add(image);
                dbContext.Entry(image).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                dbContext.SaveChanges();
            }
        }
    }
}