using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class FuelUnits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_FuelUnitsId { get; set; }
        public string FuelUnitsName { get; set; }
        public string UnitCode { get; set; }
        public Status Status { get; set; }
    }
}
