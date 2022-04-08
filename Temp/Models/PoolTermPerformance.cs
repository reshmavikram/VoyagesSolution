using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PoolTermPerformance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PoolTermPerformanceId { get; set; }
        public ICollection<Vessel> VesselList { get; set; }
        public long VesselId { get; set; }
        public DateTime PeriodEndingDate { get; set; }
        public decimal LadenSpeedEval { get; set; }
        public decimal LadenSpeedAverage { get; set; }
        public decimal LadenFuelME { get; set; }
        public decimal LadenFuelAUX { get; set; }
        public decimal LadenFuelOTHER { get; set; }
        public decimal BallastSpeedEval { get; set; }
        public decimal BallastSpeedAverage { get; set; }
        public decimal BallastFuelME { get; set; }
        public decimal BallastFuelAUX { get; set; }
        public decimal BallastFuelOTHER { get; set; }
        public decimal LoadingHSFO { get; set; }
        public decimal LoadingMGO { get; set; }
        public decimal DischargingHSFO { get; set; }
        public decimal DischargingMGO { get; set; }
        public decimal IdleHSFO { get; set; }
        public decimal IdleMGO { get; set; }
        public Status Status { get; set; }
        [ForeignKey("PoolTerms")]
        public long? PoolTermId { get; set; }
        public virtual PoolTerms PoolTerm { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
