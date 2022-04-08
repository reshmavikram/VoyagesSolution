using Data.Solution.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PassagesApprovalAudits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PassagesApprovalAuditId { get; set; }
        [ForeignKey("Voyages")]
        public long VoyagesId { get; set; }
        public virtual Voyages Voyages { get; set; }
        public bool IsInitialApproved { get; set; }
        public bool IsFinalApproved { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovalAction { get; set; }
        public DateTime ApprovalDateTime { get; set; }
        [ForeignKey("User")]
        public long ApproverId { get; set; }
        public virtual User User { get; set; }
        //public long ApproveuserUserId { get; set; }
        //public long FinalApproveuserUserId { get; set; }
    }
}
