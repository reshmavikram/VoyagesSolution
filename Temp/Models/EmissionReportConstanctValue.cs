using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class EmissionReportConstantValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_EmissionReportConstantId { get; set; }
        public string ConsumptionCategory { get; set; }
        public string FuelType { get; set; }
        public decimal Co2 { get; set; }
        public decimal NOx { get; set; }
        public decimal PM10 { get; set; }

    }
}
