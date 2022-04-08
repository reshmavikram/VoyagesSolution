using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class RoleReportMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RoleReportMapId { get; set; }
        [ForeignKey("Role")]
        public long RoleId { get; set; }
        public virtual Role Roles { get; set; }
        [ForeignKey("Reports")]
        public long ReportId { get; set; }
        public virtual Reports Reports { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
