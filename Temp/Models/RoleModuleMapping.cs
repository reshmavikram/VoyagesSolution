using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class RoleModuleMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RoleModuleMappingId { get; set; }
        [ForeignKey("RoleId")]
        public long? RoleId { get; set; }
        public virtual Role Role { get; set; }
        [ForeignKey("ModuleId")]
        public long? ModuleId { get; set; }
        public virtual Module Module { get; set; }

        [ForeignKey("PolicyId")]
        public long? PolicyId { get; set; }
        public virtual Policy Policy { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public ICollection<Role> rolesList { get; set; }
    }
}
