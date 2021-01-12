using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using iGadget.DB;
using iGadget.DB.Models;
using Microsoft.Net.Http.Headers;

namespace iGadget.App.Controllers
{
    [ApiController]
    public class API : ControllerBase
    {
        private readonly ILogger<API> _logger;

        private readonly iGadgetDataCache _db;

        public API(ILogger<API> logger, iGadgetDataCache db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        [Route("/api/rss")]
        public IEnumerable<RSSFeed> GetRSSFeeds()
        {
            return _db.Feeds.Include(a => a.Items).ToList();
        }

        [HttpGet]
        [Route("/api/weather")]
        public IEnumerable<WeatherSite> GetWeather()
        {
            return _db.WeatherSites.Include(a => a.Forecasts).ToList().Select(a => {Convert.ToSingle(Convert.ToInt32(a.Temp)); return a; }).ToList();
        }

        [HttpGet]
        [Route("/api/comics")]
        public IEnumerable<Comic> GetComics()
        {
            var comics = _db.Comics.Include(a => a.Comics).ToList();
            comics.ForEach(a => a.Comics = a.Comics.OrderByDescending(b => b.DownloadedDate).ToList());
            return comics;
        }

        [HttpGet]
        [Route("/api/images/{recommendedFileName}")]
        public FileContentResult GetImage(string recommendedFileName)
        {
            var extension = Path.GetExtension(recommendedFileName).Replace(".", "");
            var name = Path.GetFileNameWithoutExtension(recommendedFileName);
            var image = _db.Images.First(a => a.MD5.ToUpper() == name.ToUpper() && a.RecommendedExtension.ToUpper() == extension.ToUpper());
            return new FileContentResult(image.Content, MediaTypeHeaderValue.Parse(image.MIMEType));
        }        
    }
}
