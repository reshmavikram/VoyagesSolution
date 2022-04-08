using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class Machinery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_MachineryId { get; set; }
        public string MachineryType { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
