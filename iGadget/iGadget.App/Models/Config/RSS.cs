using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iGadget.App.Models.Config.RSS
{
    public class RSSFeed 
    {
        public string CronSchedule { get; set; }

        public Uri URL { get; set; }

        public int Count { get; set; }

        public Uri Site { get; set; }

        public string Name { get; set; }
    }
}