using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class WindSpeedUnits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_WindSpeedUnitsId { get; set; }
        public string WindSpeedUnitsName { get; set; }
        public string WindSpeedUnitsCode { get; set; }
    }
}
