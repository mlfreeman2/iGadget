using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iGadget.DB.Models
{
    public class PublixWeeklyAd 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public int ZIPCode { get; set; }

        public EMail EMail { get; set; }
        
        public string Subject { get; set; }
        
        public List<PublixWeeklyAdRecipient> Recipients { get; set; } = new List<PublixWeeklyAdRecipient>();

        public DateTime DownloadedDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<PublixWeeklyAdPage> AdPages { get; set; } = new List<PublixWeeklyAdPage>();
    }

    public class PublixWeeklyAdRecipient 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public string Address { get; set; }
    }

    public class PublixWeeklyAdPage 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }
        
        public int PageNumber { get; set; }
        
        public DownloadedImage Image { get; set; }
    }
}