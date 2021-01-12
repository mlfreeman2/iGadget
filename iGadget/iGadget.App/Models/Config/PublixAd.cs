using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace iGadget.App.Models.Config
{
    public class PublixAd 
    {
        public int ZIPCode { get; set; }

        public List<String> Recipients { get; set; }
        
        public string Schedule { get; set; }
    }

}

