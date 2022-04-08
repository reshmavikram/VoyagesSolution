using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class ConsumptionCategory
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_ConsumptionCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMainPropulsionEngine { get; set; }
        public string Alias { get; set; }
        [ForeignKey("EngineCategory")]
        public long EngineCategoryId { get; set; }
        public virtual EngineCategory EngineCategories { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        public bool IsFixed { get; set; }

    }
}
