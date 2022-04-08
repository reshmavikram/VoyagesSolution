using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PoolTermsConsumptionCategoryMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PoolTermsConsumptionCategoryMappingId { get; set; }
        [ForeignKey("PoolTerms")]
        public long PoolTermsId { get; set; }
        [ForeignKey("ConsumptionCategory")]
        public long ParentConsumptionCategoryId { get; set; }
        public long ChildConsumptionCategoryId { get; set; }
        public ICollection<PoolTermConsumptionCategoryFuelGrouping> PoolTermConsumptionCategoryFuelGroupingList { get; set; }
    }
}
