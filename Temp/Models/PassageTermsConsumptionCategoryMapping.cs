using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
   public class PassageTermsConsumptionCategoryMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PassageTermsConsumptionCategoryMappingId { get; set; }
        [ForeignKey("PassageTerms")]
        public long PassageTermsId { get; set; }
        [ForeignKey("ConsumptionCategory")]
        public long ParentConsumptionCategoryId { get; set; }
        public long ChildConsumptionCategoryId { get; set; }
        public long FuelTypeId { get; set; }
    }
}
