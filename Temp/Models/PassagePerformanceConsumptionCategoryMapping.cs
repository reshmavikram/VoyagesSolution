using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PassagePerformanceConsumptionCategoryMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PassagePerformanceConsumptionCategoryMappingId { get; set; }
        [ForeignKey("ConsumptionCategory")]
        public long ConsumptionCategoryId { get; set; }
        public decimal ConsumptionCategoryValue { get; set; }
        [ForeignKey("PassageTermPerformance")]
        public long PassageTermPerformanceSFPM_PassageTermPerformanceId { get; set; }
        public long PassageTerms { get; set; }
        
    }
}
