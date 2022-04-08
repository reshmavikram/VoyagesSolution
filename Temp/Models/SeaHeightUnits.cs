using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class SeaHeightUnits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_SeaHeightUnitsId { get; set; }
        public string SeaHeightUnitsName { get; set; }
        public string SeaHeightUnitsCode { get; set; }
    }
}
