using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PassageTermPerformance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PassageTermPerformanceId { get; set; }
        public string Description { get; set; }
        public decimal Speed { get; set; }
        public string Draft { get; set; }
        public int LoadConditionId { get; set; }
        public bool IsAboutClause { get; set; }
        public decimal SpeedAdjustmentLower { get; set; }
        public decimal SpeedAdjustmentUpper { get; set; }
        public decimal FuleAdjustmentLower { get; set; }
        public decimal FuleAdjustmentUpper { get; set; }
        public Status Status { get; set; }
       
        public ICollection<PassagePerformanceConsumptionCategoryMapping> PassagePerformanceConsumptionCategoryMappingList { get; set; }
        [ForeignKey("PassageTerms")]
        public long PassageTermsSFPM_PassageTermsId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        
    }
}
