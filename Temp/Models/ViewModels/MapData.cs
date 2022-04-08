using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models
{
    public class MeteoStratumDataList
    {
        public long SFPM_MeteoStratumId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string COG { get; set; }
        public string SOG { get; set; }
        public DateTimeOffset Time { get; set; }
        public long ImoNumber { get; set; }
        public string FormIdentifier { get; set; }
        public string VesselName { get; set; }
        public decimal AnalyzedWind { get; set; }
        public decimal AnalyzedWave { get; set; }
        public decimal AnalyzedCurrent { get; set; }
        public string Location { get; set; }
        //new field added for optmization tracking screen
        public decimal? rotation { get; set; }
        public decimal? windInBs { get; set; }
        public decimal? waveInDSS { get; set; }
        public decimal? lat { get; set; }
        public decimal? lng { get; set; }
    }
    public class AnalyzedWeatherDataList
    {
        public long SFPM_AnalyzedWeatherId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string COG { get; set; }
        public string SOG { get; set; }
        public DateTimeOffset Time { get; set; }
        public long ImoNumber { get; set; }
        public string FormIdentifier { get; set; }
        public string VesselName { get; set; }
        public decimal AnalyzedWind { get; set; }
        public decimal AnalyzedWave { get; set; }
        public decimal AnalyzedCurrent { get; set; }
        public string Location { get; set; }
        //new field added for optmization tracking screen
        public decimal? rotation { get; set; }
        public decimal? windInBs { get; set; }
        public decimal? waveInDSS { get; set; }
        public decimal? lat { get; set; }
        public decimal? lng { get; set; }
    }
   
}
