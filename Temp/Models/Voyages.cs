using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
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
    public class Voyages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_VoyagesId { get; set; }
        public long VoyageNumber { get; set; }
        public string VesselCode { get; set; }
        public string Description { get; set; }
        public string LoadCondition { get; set; }
        public string DeparturePort { get; set; }
        public string DepartureTimezone { get; set; }
        public string ArrivalPort { get; set; }
        public string ArrivalTimezone { get; set; }
        public DateTime ActualStartOfSeaPassage { get; set; }
        public DateTime? ActualEndOfSeaPassage { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public string IMONumber { get; set; }
        [NotMapped]
        public bool IsConflict { get; set; }
    }
    
}
