using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    [Serializable]
    public class FormUnits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_FormUnitsId { get; set; }
        [ForeignKey("Forms")]
        public long? FormId { get; set; }
        public virtual Forms Forms { get; set; }
        [ForeignKey("DistanceUnits")]
        public long? DistanceUnitsId { get; set; }
        public virtual DistanceUnits DistanceUnits { get; set; }
        [ForeignKey("VelocityUnits")]
        public long? VelocityUnitsId { get; set; }
        public virtual VelocityUnits VelocityUnits { get; set; }
        [ForeignKey("WindSpeedUnits")]
        public long? WindSpeedUnitsId { get; set; }
        public virtual WindSpeedUnits WindSpeedUnits { get; set; }
        [ForeignKey("SeaHeightUnitsDraftFwd")]
        public long? DraftFwdUnitsId { get; set; }
        public virtual SeaHeightUnits SeaHeightUnitsDraftFwd { get; set; }
        [ForeignKey("SeaHeightUnits")]
        public long? SeaHeightUnitsId { get; set; }
        public virtual SeaHeightUnits SeaHeightUnits { get; set; }
        [ForeignKey("TemperatureUnits")]
        public long? TemperatureUnitsId { get; set; }
        public virtual TemperatureUnits TemperatureUnits { get; set; }
        [ForeignKey("PressureUnits")]
        public long? PressureUnitsId { get; set; }
        public virtual PressureUnits PressureUnits { get; set; }
        [ForeignKey("DirectionUnits")]
        public long? DirectionUnitsId { get; set; }
        public virtual DirectionUnits DirectionUnits { get; set; }
        [ForeignKey("FuelUnits")]
        public long? FuelUnitsId { get; set; }
        public virtual FuelUnits FuelUnits { get; set; }
        [ForeignKey("PowerUnits")]
        public long? PowerUnitsId { get; set; }
        public virtual PowerUnits PowerUnits { get; set; }
    }
}
