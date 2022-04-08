using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class Agent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AgentId { get; set; }
        public string companyCode { get; set; }
        public string vesselCode { get; set; }
        public int voyageNo { get; set; }
        public int portCallSeq { get; set; }
        

    }
}
