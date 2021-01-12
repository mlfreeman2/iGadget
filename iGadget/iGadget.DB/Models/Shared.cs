using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using iGadget.Util.MIME;

namespace iGadget.DB.Models
{
    public class DownloadedImage 
    {
        public DownloadedImage() { }

        public DownloadedImage(byte[] content) 
        {
            Content = content;
            var mime = MIMETypes.Load();
            
            MIMEType = mime.GetMIMEType(content);
            RecommendedExtension = mime.GetRecommendedExtension(content);

            using (MD5 hashGenerator = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashValue = hashGenerator.ComputeHash(content);
                MD5 = BitConverter.ToString(hashValue).Replace("-", "").ToUpper();
            }
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public string MD5 { get; set; }

        public byte[] Content { get; set; } = new byte[0];

        public string MIMEType { get; set; }

        public string RecommendedExtension { get; set; }

        public string RecommendedFileName => $"{MD5}.{RecommendedExtension}";
    }
}