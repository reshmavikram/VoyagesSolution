using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Solution.Models
{
    [Serializable]
    public class UserRoleMapping
    {
        [System.ComponentModel.DataAnnotations.Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserRoleMappingId { get; set; }
        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User user{ get; set; }
        [ForeignKey("Role")]
        public long RoleId { get; set; }
        public virtual Role role { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}