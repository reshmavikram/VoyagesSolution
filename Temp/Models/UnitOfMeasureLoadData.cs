using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models
{
    public class UnitOfMeasureLoadData
    {
       public List<DistanceUnits> DistanceUnitsList { get; set; }
        public List<VelocityUnits> VelocityUnitsList { get; set; }
        public List<SevereWaveThresholdUnit> SevereWaveThresholdUnitList { get; set; }
        public List<WindSpeedUnits> WindSpeedUnitsList { get; set; }
        public List<SeaHeightUnits> SeaHeightUnitsList { get; set; }
        public List<FuelUnits> FuelUnitsList { get; set; }
        public List<PowerUnits> PowerUnitsList { get; set; }
        public List<ReportInPortActivityDaily> ReportInPortActivityDailyList { get; set; }
        public List<SevereWindThreshold> SevereWindThresholdList { get; set; }
        public List<VesselOffRouteDistanceUnit> VesselOffRouteDistanceUnitList { get; set; }
        public List<TemperatureUnits> TemperatureUnitsList { get; set; }
        public List<PressureUnits> PressureUnitsList { get; set; }
        public List<DirectionUnits> DirectionUnitsList { get; set; }
    }
}
