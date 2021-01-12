using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iGadget.DB.Models
{
    public class RSSFeed
    {
        [Key]
        public string Name { get; set; }

        public Uri URL { get; set; }

        public int Count { get; set; }

        public Uri Site { get; set; }

        public List<RSSFeedItem> Items { get; set; } = new List<RSSFeedItem>();
    }

    public class RSSFeedItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public int Order { get; set; }

        public Uri Link { get; set; }

        public string Title { get; set; }
    }
}