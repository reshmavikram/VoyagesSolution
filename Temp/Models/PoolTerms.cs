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
    [Serializable]
    public class PoolTerms
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PoolTermId { get; set; }
        public string TermsTitle { get; set; }
        [NotMapped]
        public List<Speed> speedList { get; set; }
        [NotMapped]
        public List<FuelUnit> FuelUnitList { get; set; }
        [NotMapped]
        public List<EvaluationType> EvaluationTypeList { get; set; }
        [NotMapped]
        public List<WeatherSource> WeatherSourceList { get; set; }
        [NotMapped]
        public List<CurrentFactor> CurrentFactorList { get; set; }
        [NotMapped]
        public List<Vessel> VesselList { get; set; }
        [NotMapped]
        public List<ConsumptionCategory> ConsumptionCategoryList { get; set; }
        public ICollection<PoolTermPerformance> PoolTermPerformanceList { get; set; }
        public ICollection<PoolTermsAssignedToVessel> PoolTermsAssignedToVessel { get; set; }
        public ICollection<PoolTermsConsumptionCategoryMapping> PoolTermsConsumptionCategoryMappingList { get; set; }
        public int SpeedId { get; set; }
        public int FuelUnitId { get; set; }
        public int DraftHeightUnitId { get; set; }
        public int EvaluationTypeId { get; set; }
        public int WeatherSourceId { get; set; }
        public int CurrentFactorId { get; set; }
        public int WindSpeedId { get; set; }
        public string WindSpeed { get; set; }
        public int WaveHeightId { get; set; }
        public string WaveHeight { get; set; }
        public string MinDayHours { get; set; }
        public string AdverseCurrentLimit { get; set; }
        public int PassageHourId { get; set; }
        public string MinPassageHours { get; set; }
        public int BadDayHoursId { get; set; }
        public string BadDayHours { get; set; }
        public decimal BallastSpeed { get; set; }
        public decimal LadenSpeed { get; set; }
        public decimal LowerPoolRange { get; set; }
        public decimal UpperPoolRange { get; set; }
        public long ConsumptionCategoryId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
