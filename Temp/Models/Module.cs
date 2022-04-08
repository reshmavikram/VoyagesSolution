using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class Module
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string AlternateModuleName { get; set; }
        public long ParentId { get; set; }
        //public long? PolicyId { get; set; }
        //public long? RoleId { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        [NotMapped]
        public long? PolicyId { get; set; }
    }
}
