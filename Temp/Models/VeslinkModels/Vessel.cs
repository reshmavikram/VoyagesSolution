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
    public class Vessel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_VesselId { get; set; }
        public string VesselCode { get; set; }
        public string VesselName { get; set; }
        public string CompanyCode { get; set; }
        public string IMONumber { get; set; }        
        public string CallSign { get; set; }
        public string Email { get; set; }
        public string Identity { get; set; }
        public long MMSI { get; set; }

        [ForeignKey("User")]
        public long? VesselOwnerId { get; set; }
        public virtual User User { get; set; }


        public string Operator { get; set; }
        public string Charterer { get; set; }
        public string  Agent { get; set; }



        [ForeignKey("VesselClassId")]
        public long? VesselClassId { get; set; }
        public VesselClass VesselClass { get; set; }

        [ForeignKey("VesselTypeId")]
        public long? VesselTypeId { get; set; }
        public VesselType VesselTypes { get; set; }


        [ForeignKey("VesselPooltypeId")]
        public long? VesselPooltypeId { get; set; }
        public VesselPooltype VesselPooltype { get; set; }


        public decimal LOA { get; set; }
        public decimal Beam { get; set; }
        public decimal MaxDraft { get; set; }
        public decimal AirDraft { get; set; }
        public decimal DesignDisplacement { get; set; }
        public decimal DWT { get; set; }
        public decimal Capacity { get; set; }
        public string Flag { get; set; }
        public int DataforTrackLine { get; set; }        
        public bool SendAlerttoVessel { get; set; }

       
        public long? TCTermsId { get; set; }
        

        public long? EnviroTechTermsId { get; set; }

       
        public long? PoolTermsId { get; set; }
      
       
        public string FieldA { get; set; }
        public string FieldB { get; set; }
        public string Remarks { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        //public bool HighFrequencySource { get; set; }
        public bool PurpleFinder { get; set; }
        public bool Stratumfive { get; set; }
        public bool SATAIS { get; set; }
        public bool Shiplink { get; set; }
        public bool FleetManager { get; set; }

        public long? VeslinkApiVesselFormStatus { get; set; }

        [NotMapped]
        public string VesselTypeCode { get; set; }
        [NotMapped]
        [JsonProperty(PropertyName ="VesselType")]
        public string VesselTypeName { get; set; }
        [NotMapped]
        public string VesselNameWithStatus { get; set; }
    }
}
