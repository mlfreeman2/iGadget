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
    public class PageController : ControllerBase
    {

        private readonly ILogger<API> _logger;

        private readonly iGadgetDataCache _db;

        public PageController(ILogger<API> logger, iGadgetDataCache db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        [Route("/publix")]
        public ActionResult PublixLatestAd() 
        {
            var publixAd = _db.PublixWeeklyAds.Include(a => a.AdPages).ThenInclude(a => a.Image).OrderByDescending(a => a.DownloadedDate).FirstOrDefault();
            if (publixAd == null) 
            {
                return NotFound();
            }
            var html = $"<html><head><title>{publixAd.Subject}</title></head><body><h1>{publixAd.Subject}</h1>";
            foreach (var page in publixAd.AdPages.OrderBy(a => a.PageNumber)) 
            {
                html += $"<img src=\"api/images/{page.Image.RecommendedFileName}\" />";
            }
            return Content(html, "text/html");
        }

        [HttpGet]
        [Route("/inplay")]
        public ActionResult InPlayDaily() 
        {
            var inPlay = _db.InPlayDailyBriefings.OrderByDescending(a => a.DownloadDate).FirstOrDefault();
            if (inPlay == null) 
            {
                return NotFound();
            }

            var body = $"<html><head><title>{inPlay.Title}</title></head><body>{inPlay.HTML}</body></html>";
            return Content(body, "text/html");
        }

    }

}