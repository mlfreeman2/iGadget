using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iGadget.DB.Models
{
    public class EMail 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public bool Sent { get; set; }

        public DateTime? SentDate { get; set; }
        
        public string From { get; set; }

        public List<EMailRecipient> Bcc { get; set; } = new List<EMailRecipient>();

        public List<EMailAttachment> Attachments { get; set; } = new List<EMailAttachment>();

        public string Subject { get; set; }

        public string TextBody { get; set; }

        public string HTMLBody { get; set; }
    }

    public class EMailRecipient 
    { 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public string Address { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name)) 
            {
                return $"{Name} <{Address}>";
            }
            return Address;
        }
    }

    public class EMailAttachment 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public string FileName { get; set; }

        public string MIMEType { get; set; }

        public string ContentDisposition { get; set; } = "attachment";

        public byte[] Content { get; set; }
    }
}