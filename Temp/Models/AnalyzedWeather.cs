using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class AnalyzedWeather
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_AnalyzedWeather_Id { get; set; }
        [ForeignKey("Forms")]
        public long FormId { get; set; }
        public virtual Forms Forms { get; set; }
        public string AnalyzedWind { get; set; }
        public string AnalyzedWave { get; set; }
        public string AnalyzedCurrent { get; set; }
        public string AnalyzedWindDirection { get; set; }
        public string AnalyzedWaveDiection { get; set; }
        public string AnalyzedSeaHeight { get; set; }
        public string AnalyzedSwellDirection { get; set; }
        public string AnalyzedSwellHeight { get; set; }
        public string AnalyzedCurrentDirection { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public decimal? AnalyzedSeaCurrentSpeedInKnots { get; set; }

        public bool IsWindExceedThresholdHour { get; set; }
        public double MinWindBadWeatherThreshold { get; set; }
        public double MaxWindGoodWeatherThreshold { get; set; }
        public bool IsWaveExceedThresholdHour { get; set; }
        public double MinWaveBadWeatherThreshold { get; set; }
        public double MaxWaveGoodWeatherThreshold { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool Is24Hour { get; set; }

        public DateTime CalculatedTimeStamp { get; set; }
        public decimal Bearing { get; set; }
    }
}