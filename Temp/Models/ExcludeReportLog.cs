using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class ExcludeReportLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_ExcludeReportLogId { get; set; }
        [ForeignKey("VoyagesId")]
        public long? VoyagesId { get; set; }
        public virtual Voyages Voyages { get; set; }
        [ForeignKey("Forms")]
        public long? FormId { get; set; }
        public virtual Forms Forms { get; set; }
        public long ReportId { get; set; }
        [ForeignKey("ReportId")]
        public virtual ExcludeReport ExcludeReport { get; set; }
        [ForeignKey("UserId")]
        public long UserId { get; set; }
        public virtual User User { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool Excluded { get; set; }
        public string Remarks { get; set; }
        [NotMapped]
        public List<ExcludeReport> excludesList { get; set; }
    }
}
