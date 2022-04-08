using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class NOXPMFactor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_NOX_PM_FactorId { get; set; }
        [ForeignKey("EngineCategories")]
        public long EngineCategoryId { get; set; }
        [ForeignKey("FuelCategories")]
        public long FuelCategory { get; set; }
        public virtual FuelCategory FuelCategories { get; set; }
        public virtual EngineCategory EngineCategories { get; set; }
        public decimal  NOXFactor { get; set; }
        public decimal PMFactor { get; set; }        
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
    }
}
