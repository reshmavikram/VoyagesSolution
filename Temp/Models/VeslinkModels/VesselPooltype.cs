using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    [Serializable]
    public class VesselPooltype
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_VesselPooltypeId { get; set; }
        public string VesselPooltypeCode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        [ForeignKey("VesselTypeId")]
        public long? VesselTypeId { get; set; }
        public virtual  VesselType VesselType { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }

    }
}
