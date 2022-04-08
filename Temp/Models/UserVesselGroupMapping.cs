using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    [Serializable]
    public class UserVesselGroupMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserVesselGroupMappingId { get; set; }
        [ForeignKey("UserId")]
        public long UserId { get; set; }
        public virtual User user { get; set; }
        [ForeignKey("VesselGroupId")]
        public long VesselGroupId { get; set; }
        public virtual VesselGroup VesselGroup { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
