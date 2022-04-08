using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models
{
    public class PassageDataChartViewModel
    {
       public List<PassageDataViewModel> PassageData { get; set; }
    }
    public class PassageDataViewModel
    {
        public long IMONumber { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public List<MeteoStratumDataList> MeteostratumDataList { get; set; }
        public List<AnalyzedWeatherDataList> AnalyzedWeatherDataList { get; set; }
    }

}
