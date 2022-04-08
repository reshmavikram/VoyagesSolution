using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class TemperatureUnits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_TemperatureUnitsId { get; set; }
        public string TemperatureUnitsName { get; set; }
        public string TemperatureUnitsCode { get; set; }
    }
}
