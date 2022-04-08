using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class BunkerVeslinkFuelType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_BunkerVeslinkFuelTypeId { get; set; }
        public long VeslinkFormId { get; set; }
        public string VeslinkFormType { get; set; }
    }
}
