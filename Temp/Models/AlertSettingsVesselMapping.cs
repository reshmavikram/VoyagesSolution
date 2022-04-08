using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class AlertSettingsVesselMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_AlertSettingsVesselMappingId { get; set; }
        public long VesselId { get; set; }
        public long VesselGroupId { get; set; }
        public long VesselClassId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        [ForeignKey("AlertSettings")]
        public long AlertSettingId { get; set; }
        public virtual AlertSettings AlertSettings { get; set; }


    }
}
