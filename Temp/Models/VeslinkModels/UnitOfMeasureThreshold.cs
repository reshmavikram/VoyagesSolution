using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Solution.Models
{
    public class UnitOfMeasureThreshold : CommonField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UnitOfMeasureThresholdId { get; set; }
        public virtual DistanceUnits DistanceUnits { get; set; }
        [ForeignKey("DistanceUnits")]
        public long? DistanceUnitsId { get; set; }
        public virtual VelocityUnits VelocityUnits { get; set; }
        [ForeignKey("VelocityUnits")]
        public long? VelocityUnitsId { get; set; }
        public virtual WindSpeedUnits WindSpeedUnits { get; set; }
        [ForeignKey("WindSpeedUnits")]
        public long? WindSpeedUnitsId { get; set; }
        public virtual SeaHeightUnits SeaHeightUnits { get; set; }
        [ForeignKey("SeaHeightUnits")]
        public long? SeaHeightUnitsId { get; set; }
        public virtual FuelUnits FuelUnits { get; set; }
        [ForeignKey("FuelUnits")]
        public long? FuelUnitsId { get; set; }
        public virtual PowerUnits PowerUnits { get; set; }
        [ForeignKey("PowerUnits")]
        public long? PowerUnitsId { get; set; }
        public Report ReportInPortActivityDaily { get; set; }
        public long LateArrivaStatuslInMinute { get; set; }
        public long LateDepartureStatusInMinute { get; set; }
        public long LookAheadInHour { get; set; }
        public long SevereWindThreshold {get;set;}
        public virtual SevereWindThreshold SevereWindThresholdUnits { get; set; }
        [ForeignKey("SevereWindThreshold")]
        public long? SevereWindThresholdUnitsId { get; set; }
        public long SevereWaveThreshold { get; set; }
        public virtual SevereWaveThresholdUnit SevereWaveThresholdUnits { get; set; }
        [ForeignKey("SevereWaveThresholdUnit")]
        public long? SevereWaveThresholdUnitsId { get; set; }
        public long PositionReportIntervalInHour {get;set;}
        public long OverduePositionReportThresholdInHour {get;set;}
        public long PassageTermFuelDeviation {get;set;}
        public long PassageTermDistanceDeviation {get;set;}
        public long PassageTermTimeDeviation {get;set;}
        public long VesselOffRouteDistance {get;set;}
        public virtual VesselOffRouteDistanceUnit VesselOffRouteDistanceUnits { get; set; }
        [ForeignKey("VesselOffRouteDistanceUnit")]
        public long? VesselOffRouteDistanceUnitsId { get; set; }
        public long OffETAProbability {get;set;}
        public long SOELimitThreshold {get;set;}
        public long HFAlertThresholdInHour {get;set; }
        public long NumberOfStratumPosition { get; set; }
        public decimal SpeedThreasholdForPositionWarningDistanceAPI { get; set; }
        //[ForeignKey("TemperatureUnits")]
        //public long? TemperatureUnitsId { get; set; }
        //public virtual TemperatureUnits TemperatureUnits { get; set; }
        //[ForeignKey("PressureUnits")]
        //public long? PressureUnitsId { get; set; }
        //public virtual PressureUnits PressureUnits { get; set; }
        //[ForeignKey("DirectionUnits")]
        //public long? DirectionUnitsId { get; set; }
        //public virtual DirectionUnits DirectionUnits { get; set; }

    }
}
