using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PoolTermConsumptionCategoryFuelGrouping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PoolTermConsumptionCategoryFuelGroupingId { get; set; }
        public long PoolTermConsumptionCategoryMappingId { get; set; }
        [ForeignKey("PoolTermConsumptionCategoryMappingId")]
        public virtual PoolTermsConsumptionCategoryMapping PoolTermsConsumptionCategoryMapping { get; set; }
        public long FuelTypeId { get; set; }
    }
}
