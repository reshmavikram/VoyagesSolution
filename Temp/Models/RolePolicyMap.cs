using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Solution.Models
{
    public class RolePolicyMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RolePolicyMapId { get; set; }
        [ForeignKey("Role")]
        public long RoleId { get; set; }
        public virtual Role Roles { get; set; }
        [ForeignKey("Policy")]
        public long PolicyId { get; set; }
        public virtual Policy Policies { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
