using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iGadget.DB.Models
{
    public class WeatherSite
    {
        [Key]
        public string Name { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }
        
        public Uri URL { get; set; }

        public List<Forecast> Forecasts { get; set; } = new List<Forecast>();

        public string TextDescription { get; set; }

        public long Temp { get; set; }

        public DateTime Requested { get; set; }

        public long Humidity { get; set; }

        public long WindSpeed { get; set; }

        public DateTimeOffset LatestObservationTime { get; set; }

        public string WindDirection { get; set; }

        public Uri Icon { get; set; }
    }

    public class Forecast
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }
        
        public DateTimeOffset ValidDate { get; set; }
        
        public bool IsDaytime { get; set; }

        public string DetailedForecast { get; set; }

        public string Name { get; set; }

        public Uri IconURL { get; set; }

        public long Temperature { get; set; } 
    }
}