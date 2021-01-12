using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iGadget.App.Models.Config
{
    public class NWSSite
    {
        public string Name { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }
        
        public Uri URL { get; set; }

        public string ForecastSchedule { get; set; }

        public string ObservationSchedule { get; set; }
    }
}