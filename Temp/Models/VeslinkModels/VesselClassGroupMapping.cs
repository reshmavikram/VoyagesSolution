using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class VesselClassGroupMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_VesselClassGroupMappingId { get; set; }
        [ForeignKey("Vessel")]
        public long VesselId { get; set; }
        [ForeignKey("VesselClass")]
        public long VesselClassId { get; set; }
        [ForeignKey("VesselGroup")]
        public long VesselGroupId { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
