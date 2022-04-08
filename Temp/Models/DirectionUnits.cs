using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class DirectionUnits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_DirectionUnitsId { get; set; }
        public string DirectionUnitsName { get; set; }
        public string DirectionUnitsCode { get; set; }
    }
}
