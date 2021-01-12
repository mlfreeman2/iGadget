using System;
using System.ComponentModel.DataAnnotations;

namespace iGadget.DB.Models 
{
    public class InPlayDailyBriefing
    {
        [Key]
        public DateTime DownloadDate { get; set; }

        public string Title { get; set; }
        
        public string HTML { get; set; }

        public EMail EMail { get; set; }
    }
}