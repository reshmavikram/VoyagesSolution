using Data.Solution.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class Reason
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ReasonId { get; set; }
        public string Name { get; set; }
        public ICollection<SubReason> Subreasons { get; set; }
    }
}
