using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class VelocityUnits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_VelocityUnitsId { get; set; }
        public string VelocityUnitsName { get; set; }
        public string VelocityUnitsCode { get; set; }
    }
}
