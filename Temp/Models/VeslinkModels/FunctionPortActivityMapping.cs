using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Solution.Models
{
    public class FunctionPortActivityMapping
    {
        [System.ComponentModel.DataAnnotations.Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FunctionPortActivityMappingId { get; set; }
        [ForeignKey("Function")]
        public long FunctionId { get; set; }
        public virtual Function function { get; set; }
        [ForeignKey("PortActivity")]
        public long PortActivityId { get; set; }
        public virtual PortActivity portActivity { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
