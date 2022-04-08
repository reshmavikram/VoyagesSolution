using Data.Solution.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PassageTerms
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PassageTermsId { get; set; }
        public string TermsTitle { get; set; }
        
        [NotMapped]
        public List<Speed> SpeedList { get; set; }
        [NotMapped]
        public List<FuelUnit> FuelUnitList { get; set; }
        [NotMapped]
        public List<DraftLimit> DraftLimitList { get; set; }
        [NotMapped]
        public List<EvaluationType> EvaluationTypeList { get; set; }
        [NotMapped]
        public List<WeatherSource> WeatherSourceList { get; set; }
        [NotMapped]
        public List<CurrentFactor> CurrentFactorList { get; set; }
        [NotMapped]
        public List<Vessel> VesselList { get; set; }
        [NotMapped]
        public List<VesselGroup> VesselGroupList { get; set; }
        [NotMapped]
        public List<VesselClass> VesselClassList { get; set; }
        [NotMapped]
        public List<TermsType> TermsTypeList { get; set; }
        public ICollection<PassageTermVesselMappings> PassageTermVesselList { get; set; }
        public ICollection<PassageTermsConsumptionCategoryMapping> PassageTermsConsumptionCategoryMappingList { get; set; }
        public ICollection<PassageTermPerformance> PassageTermPerformanceList { get; set; }
        public long VesselTypeId { get; set; }
        public int TermsTypeId { get; set; }
        public int SpeedId { get; set; }
        public int FuelUnitId { get; set; }
        public decimal FwdDraftLimit { get; set; }
        public decimal AftDraftLimit { get; set; }
        public decimal AvgDraftLimit { get; set; }
        public int EvaluationTypeId { get; set; }
        public int WeatherSourceId { get; set; }
        public int CurrentFactorId { get; set; }
        public int WindSpeedId { get; set; }
        public decimal WindSpeed { get; set; }
        public int WaveHeightId { get; set; }
        public decimal WaveHeight { get; set; }
        public decimal MinDayHours { get; set; }
        public string AdverseCurrentLimit { get; set; }
        public int PassageHourId { get; set; }
        public decimal MinPassageHours { get; set; }
        public int BadDayHoursId { get; set; }
        public decimal BadDayHours { get; set; }
        public int TimeLostId { get; set; }
        public int TimeGainedId { get; set; }
        public int FuelOverConsumed { get; set; }
        public int FuelOverConsumedSpeedConsidered { get; set; }
        public int FuelUnderConsumed { get; set; }
        public int FuelunderConsumedSpeedConsidered { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public int AnalzedWeatherCalculationHours { get; set; }
    }

}
