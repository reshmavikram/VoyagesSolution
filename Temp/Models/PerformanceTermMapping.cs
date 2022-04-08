using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PerformanceTermMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PerformanceTermMappingId { get; set; }
        [ForeignKey("PassageTerms")]
        public long PassageTermsId { get; set; }
        [ForeignKey ("Performance")]
        public long PerformanceId { get; set; }
    }
}
