using System;
using System.Collections.Generic;

namespace iGadget.App.Models.Config 
{
    public class InPlay 
    {
        public Uri URL { get; set; }

        public string Selector { get; set; }

        public string Schedule { get; set; }

        public List<String> Recipients { get; set; }
    }
}