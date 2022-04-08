using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Solution.Models
{
   public class DelayType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DelayTypeId { get; set; }
        //public long DelayTypeVeslinkId { get; set; }
        public string Name { get; set; }
        public bool IsVesselStopped { get; set; }
        public string Code { get; set; }
        public ICollection<Reason> Reasons { get; set; }
    }
}
