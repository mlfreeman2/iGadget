using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iGadget.DB.Models
{
    public class Comic
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

        public Uri URL { get; set; }

        public List<ComicIssue> Comics { get; set; } = new List<ComicIssue>();
    }

    public class ComicIssue 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }
        
        public DateTime? DownloadedDate { get; set; } = DateTime.Now;

        public string Alt { get; set; }

        public string Title { get; set; }

        public string RecommendedFileName { get; set; }

        public DownloadedImage Image { get; set; }
    }
}