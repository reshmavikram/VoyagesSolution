using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class PassageTermVesselMappings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PassageTermVesselMappingId { get; set; }
        [ForeignKey("PassageTerms")]
        public long PassageTermsId { get; set; }      
     
        public long VesselId { get; set; }
   
    }
}
