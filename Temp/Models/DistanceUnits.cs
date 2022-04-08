using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class DistanceUnits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_DistanceUnitsId { get; set; }
        public string DistanceUnitsName { get; set; }
        public string DistanceUnitsCode { get; set; }
    }
}
