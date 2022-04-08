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
    public class VesselOwner
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long VesselOwnerId { get; set; }
        public string OwnerName { get; set; }
        public string Email { get; set; }
        [ForeignKey("Roles")]
        public long RoleId { get; set; }
        public virtual Role Roles { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
