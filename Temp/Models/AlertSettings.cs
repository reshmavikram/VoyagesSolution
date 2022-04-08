using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class AlertSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_AlertSettingId { get; set; }
        public string AlertName { get; set; }
        public bool IsSubscribe { get; set; }
        public long? MappingTypeId { get; set; }
        [ForeignKey("MappingTypeId")]
        public virtual AlertSettingMappingType AlertSettingMappingTypes { get; set; }
        public int Rule { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
