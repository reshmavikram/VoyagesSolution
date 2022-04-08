using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Solution.Models
{
    public class CompanyCategoryMapping
    {
        [System.ComponentModel.DataAnnotations.Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CompanyCategoryMappingId { get; set; }
        [ForeignKey("Companies")]
        public long CompanyId { get; set; }
        public virtual Company Companies { get; set; }
        [ForeignKey("Category")]
        public long CategoryId { get; set; }
        public virtual Category category { get; set; }
    }
}
