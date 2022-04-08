using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PositionWarningAudit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PositionWarningAuditId { get; set; }
        [ForeignKey("PositionWarning")]
        public long PositionWarningId { get; set; }
        public bool IsApproved { get; set; }
        [ForeignKey("Users")]
        public long ReviewedBy { get; set; }
        public DateTime? ReviewDateTime { get; set; }
        [NotMapped]
        public string ReviewedName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
