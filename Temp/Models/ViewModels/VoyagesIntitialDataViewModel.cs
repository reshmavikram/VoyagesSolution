using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    [Serializable]
     public class VoyagesIntitialDataViewModel
    {
        public string Source { get; set; }
        public string EditedBy { get; set; }
        public string Type { get; set; }
        public string IconType { get; set; }
        public DateTimeOffset? DateAndTime { get; set; }
        public string TimeZone { get; set; }
        public bool IsConflict { get; set; }
        public Forms Form { get; set; }
        public bool ExcludedFromPool { get; set; }
        public bool ExcludedFromTC { get; set; }
        public string Positions { get; set; }
        public string HrsReportDatetime { get; set; }
        public string AnalyzedWind { get; set; }
        public string AnalyzedWave { get; set; }
        public string AnalyzedWindDirection { get; set; }
        public string AnalyzedWaveDiection { get; set; }
        public string AnalyzedCurrent { get; set; }
        public bool IsEventExists { get; set; }
        public bool IsPositionWarningExists { get; set; }
    }

}
