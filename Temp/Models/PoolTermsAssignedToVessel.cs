using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PoolTermsAssignedToVessel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PoolTermsAssignedToVesselId { get; set; }
        [ForeignKey("Vessel")]
        public long VesselId { get; set; }
        public Vessel Vessels { get; set; }
        public decimal EvaluationSpeedBallast { get; set; }
        public decimal EvaluationSpeedLaden { get; set; }
        [ForeignKey("PoolTerms")]
        public long? PoolTermId { get; set; }
        public virtual PoolTerms PoolTerm { get; set; }

    }
}
