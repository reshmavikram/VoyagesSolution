using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data.Solution.Models
{
    [Serializable]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RoleId { get; set; }
        public string RoleName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        public List<RolePolicyMapping> rolePolicyMap { get; set; }
        public List<RoleReportMapping> roleReportMap { get; set; }
        public List<UserRoleMapping> UserRoleMapping { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public List<Module> modulesList { get; set; }
    }
}
