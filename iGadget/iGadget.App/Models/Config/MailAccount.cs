using System;

namespace iGadget.App.Models.Config 
{
    public class MailAccount 
    {
        public string Server { get; set; }

        public int Port { get; set; }

        public bool TLS { get; set; }

        public string From { get; set; }

        public string User { get; set; }

        public string Password { get; set; }
    }
}